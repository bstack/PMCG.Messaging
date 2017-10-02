using log4net;
using PMCG.Messaging.Client.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace PMCG.Messaging.Client
{
	public class Bus : IBusController, IBus
	{
		private readonly BusConfiguration c_configuration;
		private readonly ILog c_logger;
		private readonly IConnectionManager c_connectionManager;
		private readonly BlockingCollection<Publication> c_publicationQueue;
		private readonly Task[] c_publisherTasks;


		public Bus(
			BusConfiguration configuration)
		{
			this.c_logger = LogManager.GetLogger(this.GetType());
			this.c_logger.Info("ctor Starting");

			Check.RequireArgumentNotNull("configuration", configuration);

			this.c_configuration = configuration;
			this.c_connectionManager = ServiceLocator.GetConnectionManager(this.c_configuration);
			this.c_publicationQueue = new BlockingCollection<Publication>();
			this.c_publisherTasks = new Task[this.c_configuration.NumberOfPublishers];

			this.c_logger.Info("ctor Completed");
		}


		public void Connect()
		{
			this.c_logger.Info("Connect Starting");
			this.c_connectionManager.Open(numberOfTimesToTry: 3);

            if(!this.c_connectionManager.IsOpen)
            {
                this.c_logger.Error("Failed to connect to server, connection manager connection is closed");
                throw new ApplicationException("Failed to connect to server, connection manager connection is closed");
            }

			this.c_logger.Info("Connect About to create publisher tasks");
			for (var _index = 0; _index < this.c_publisherTasks.Length; _index++)
			{
				var _publisher = new Publisher(this.c_connectionManager.Connection, this.c_publicationQueue);
				this.c_publisherTasks[_index] = _publisher.Start();
			}

			this.c_logger.Info("Connect About to create consumers");
			for (var _index = 0; _index < this.c_configuration.NumberOfConsumers; _index++)
			{
				var _consumer = new Consumer(this.c_connectionManager.Connection, this.c_configuration);
				_consumer.Start();
			}

			this.c_logger.Info("Connect Completed");
		}


		public void Close()
		{
			this.c_logger.Info("Close Starting");
			this.c_connectionManager.Close();
			this.c_logger.Info("Close Completed");
		}


		public Task<PMCG.Messaging.PublicationResult> PublishAsync<TMessage>(
			TMessage message)
			where TMessage : Message
		{
			this.c_logger.DebugFormat("PublishAsync Publishing message ({0}) with Id {1}", message, message.Id);

			var _result = new TaskCompletionSource<PMCG.Messaging.PublicationResult>();
			if (!this.DoesPublicationConfigurationExist(message))
			{
				_result.SetResult(
					new PMCG.Messaging.PublicationResult(
						PMCG.Messaging.PublicationResultStatus.NoConfigurationFound,
						message));
			}
			else
			{
				var _thisPublicationsPublications = this.c_configuration.MessagePublications[message.GetType()]
					.Configurations
					.Select(deliveryConfiguration =>
						new Publication(
							deliveryConfiguration,
							message,
							new TaskCompletionSource<PublicationResult>()));

				var _tasks = new List<Task<PublicationResult>>();
				foreach (var _publication in _thisPublicationsPublications)
				{
					this.c_publicationQueue.Add(_publication);
					_tasks.Add(_publication.ResultTask);
				}
				Task.WhenAll(_tasks).ContinueWith(taskResults =>
				{
					if (taskResults.IsFaulted) { _result.SetException(taskResults.Exception); }
					else { _result.SetResult(this.CreateNonFaultedPublicationResult(message, taskResults)); }
				});
			}

			this.c_logger.Debug("PublishAsync Completed");
			return _result.Task;
		}


		private bool DoesPublicationConfigurationExist<TMessage>(
			TMessage message)
			where TMessage : Message
		{
			if (!this.c_configuration.MessagePublications.HasConfiguration(message.GetType()))
			{
				this.c_logger.WarnFormat("No configuration exists for publication of message ({0}) with Id {1}", message, message.Id);
				Check.Ensure(!typeof(Command).IsAssignableFrom(typeof(TMessage)), "Commands must have a publication configuration");
				return false;
			}

			return true;
		}


		private PMCG.Messaging.PublicationResult CreateNonFaultedPublicationResult(
			Message message,
			Task<PublicationResult[]> publicationResults)
		{
			var _allGood = publicationResults.Result.All(result => result.Status == PublicationResultStatus.Acked);
			var _status = _allGood ? PMCG.Messaging.PublicationResultStatus.Published : PMCG.Messaging.PublicationResultStatus.NotPublished;

			return new PMCG.Messaging.PublicationResult(_status, message);
		}
	}
}
