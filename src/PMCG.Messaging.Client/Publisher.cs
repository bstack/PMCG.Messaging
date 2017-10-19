﻿using log4net;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PMCG.Messaging.Client
{
	public class Publisher
	{
		private readonly ILog c_logger;
		private readonly BlockingCollection<Publication> c_publicationQueue;
		private readonly IModel c_channel;
		private readonly ConcurrentDictionary<ulong, Publication> c_unconfirmedPublications;


		public Publisher(
			IConnection connection,
			BlockingCollection<Publication> publicationQueue)
		{
			this.c_logger = LogManager.GetLogger(this.GetType());
			this.c_logger.Info("ctor Starting");

			this.c_publicationQueue = publicationQueue;

			this.c_channel = connection.CreateModel();
			this.c_channel.ConfirmSelect();
			this.c_channel.ModelShutdown += (m, args) => this.OnChannelShutdown(args);
			this.c_channel.BasicAcks += (m, args) => this.OnChannelAcked(args);
			this.c_channel.BasicNacks += (m, args) => this.OnChannelNacked(args);

			this.c_unconfirmedPublications = new ConcurrentDictionary<ulong, Publication>();

			this.c_logger.Info("ctor Completed");
		}


		public Task Start()
		{
			this.c_logger.Info("Start Starting");

			var _result = new Task(
				() =>
					{
						try
						{
							foreach (var _publication in this.c_publicationQueue.GetConsumingEnumerable())
							{
								this.Publish(_publication);
							}
						}
						catch (Exception exception)
						{
							this.c_logger.ErrorFormat("Start Exception : {0}", exception.InstrumentationString());
							throw;
						}
					},
				TaskCreationOptions.LongRunning);
			_result.Start();

			this.c_logger.Info("Start Completed publishing");
			return _result;
		}


		private void Publish(
			Publication publication)
		{
			this.c_logger.DebugFormat("Publish About to publish message with Id {0} to exchange {1}", publication.Id, publication.ExchangeName);

		
			var _properties = this.c_channel.CreateBasicProperties();
			_properties.ContentType = "application/json";
			_properties.DeliveryMode = publication.DeliveryMode;
			_properties.Type = publication.TypeHeader;
			_properties.MessageId = publication.Id;
			// Only set if null, otherwise library will blow up, default is string.Empty, if set to null will blow up in library
			if (publication.CorrelationId != null) { _properties.CorrelationId = publication.CorrelationId; }

			var _messageJson = JsonConvert.SerializeObject(publication.Message);
			var _messageBody = Encoding.UTF8.GetBytes(_messageJson);

			var _deliveryTag = this.c_channel.NextPublishSeqNo;

			try
			{
				this.c_unconfirmedPublications.TryAdd(_deliveryTag, publication);
				if (this.c_channel.IsOpen)
				{
					// NOTE: There is issues around here in relation to when automatic recovery happens:
					//		This call does not throw an exception
					//		OnChannelAcked never gets called as connection to server gone
					//		Results in messages get lost during publishing and automatic recovery happening
					// See for more detailhttps://groups.google.com/forum/#!topic/rabbitmq-users/HrJDi9Octr4
					this.c_channel.BasicPublish(publication.ExchangeName, publication.RoutingKey, _properties, _messageBody);

					this.c_logger.DebugFormat("Publish Completed publishing message with Id {0} to exchange {1}", publication.Id, publication.ExchangeName);
				}
				else
				{
					Task.Delay(1000).Wait(); // We delay for short period to allow automatic recovery to reconnect, otherwise will keep looping adding/removing from blocking colection
					this.c_unconfirmedPublications.TryRemove(_deliveryTag, out Publication removedPublication);
					this.c_publicationQueue.Add(publication);

					this.c_logger.WarnFormat("Publish Failed publishing message with Id {0} to exchange {1}, channel was closed", publication.Id, publication.ExchangeName);
				}
			}
			catch (Exception exception)
			{
				Task.Delay(1000).Wait(); // We delay for short period to allow automatic recovery to reconnect, otherwise will keep looping adding/removing from blocking colection
				this.c_unconfirmedPublications.TryRemove(_deliveryTag, out Publication removedPublication);
				this.c_publicationQueue.Add(publication);

				this.c_logger.ErrorFormat("Publish Failed publishing message with Id {0} to exchange {1}, unexpected exception {2}", publication.Id, publication.ExchangeName, exception.Message);
			}
		}


		private void OnChannelShutdown(
			ShutdownEventArgs reason)
		{
			this.c_logger.WarnFormat("OnChannelShutdown Starting, code = {0} and text = {1}", reason.ReplyCode, reason.ReplyText);

			var _highestDeliveryTag = this.c_unconfirmedPublications
				.Keys
				.OrderByDescending(deliveryTag => deliveryTag)
				.FirstOrDefault();
			if (_highestDeliveryTag > 0)
			{
				var _context = string.Format("Code = {0} and text = {1}", reason.ReplyCode, reason.ReplyText);
				this.ProcessDeliveryTags(
					true,
					_highestDeliveryTag,
					publication => publication.SetResult(PublicationResultStatus.ChannelShutdown, _context));
			}

			this.c_logger.WarnFormat("OnChannelShutdown Completed, code = {0} and text = {1}", reason.ReplyCode, reason.ReplyText);
		}


		private void OnChannelAcked(
			BasicAckEventArgs args)
		{
			this.c_logger.DebugFormat("OnChannelAcked Starting, is multiple = {0} and delivery tag = {1}", args.Multiple, args.DeliveryTag);

			this.ProcessDeliveryTags(
				args.Multiple,
				args.DeliveryTag,
				publication => publication.SetResult(PublicationResultStatus.Acked));

			this.c_logger.DebugFormat("OnChannelAcked Completed, is multiple = {0} and delivery tag = {1}", args.Multiple, args.DeliveryTag);
		}


		private void OnChannelNacked(
			BasicNackEventArgs args)
		{
			this.c_logger.WarnFormat("OnChannelNacked Starting, is multiple = {0} and delivery tag = {1}", args.Multiple, args.DeliveryTag);

			this.ProcessDeliveryTags(
				args.Multiple,
				args.DeliveryTag,
				publication => publication.SetResult(PublicationResultStatus.Nacked));

			this.c_logger.WarnFormat("OnChannelNacked Completed, is multiple = {0} and delivery tag = {1}", args.Multiple, args.DeliveryTag);
		}


		private void ProcessDeliveryTags(
			bool isMultiple,
			ulong highestDeliveryTag,
			Action<Publication> action)
		{
			// Critical section -What if an ack followed by a nack and the two trying to do work at the same time
			var _deliveryTags = new[] { highestDeliveryTag };
			if (isMultiple)
			{
				_deliveryTags = this.c_unconfirmedPublications
					.Keys
					.Where(deliveryTag => deliveryTag <= highestDeliveryTag)
					.ToArray();
			}

			Publication _publication = null;
			foreach (var _deliveryTag in _deliveryTags)
			{
				if (!this.c_unconfirmedPublications.ContainsKey(_deliveryTag)) { continue; }

				this.c_unconfirmedPublications.TryRemove(_deliveryTag, out _publication);
				action(_publication);
			}
		}
	}
}