using log4net;
using PMCG.Messaging.Client.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;


namespace PMCG.Messaging.Client
{
	public class Consumer
	{
		private readonly ILog c_logger;
		private readonly BusConfiguration c_configuration;
		private readonly IModel c_channel;
		private readonly ConsumerMessageProcessor c_messageProcessor;

		private EventingBasicConsumer c_consumer;


		public Consumer(
			IConnection connection,
			BusConfiguration configuration)
		{
			this.c_logger = LogManager.GetLogger(this.GetType());
			this.c_logger.Info("ctor Starting");

			this.c_configuration = configuration;

			this.c_logger.Info("ctor About to create channel");
			this.c_channel = connection.CreateModel();
			this.c_channel.ModelShutdown += (m, args) => this.OnChannelShutdown(args);
			this.c_channel.BasicQos(0, this.c_configuration.ConsumerMessagePrefetchCount, false);

			this.c_logger.Info("ctor About to create consumer message processor");
			this.c_messageProcessor = new ConsumerMessageProcessor(this.c_configuration);

			this.c_logger.Info("ctor Completed");
		}


		public void Start()
		{
			this.c_logger.Info("Start Starting");

			this.c_consumer = new EventingBasicConsumer(this.c_channel);
			this.c_consumer.Received += (m, args) => { this.c_messageProcessor.Process(this.c_channel, args); };
			this.EnsureTransientQueuesExist();
			this.CreateAndConfigureConsumer();

			this.c_logger.Info("Start Completed consuming");
		}


		private void OnChannelShutdown(
			ShutdownEventArgs reason)
		{
			this.c_logger.InfoFormat("OnChannelShutdown Code = {0} and text = {1}", reason.ReplyCode, reason.ReplyText);
		}


		private void EnsureTransientQueuesExist()
		{
			foreach (var _configuration in this.c_configuration.MessageConsumers.GetTransientQueueConfigurations())
			{
				this.c_logger.InfoFormat("EnsureTransientQueuesExist Verifying for transient queue {0}", _configuration.QueueName);
				this.c_channel.QueueDeclare(_configuration.QueueName, false, false, true, null);
				this.c_channel.QueueBind(_configuration.QueueName, _configuration.ExchangeName, string.Empty);
			}
		}


		private void CreateAndConfigureConsumer()
		{
			foreach (var _queueName in this.c_configuration.MessageConsumers.GetDistinctQueueNames())
			{
				this.c_logger.InfoFormat("CreateAndConfigureConsumer Consume for queue {0}", _queueName);
				var _consumerTag = this.c_channel.BasicConsume(_queueName, false, this.c_consumer);
				this.c_logger.InfoFormat("CreateAndConfigureConsumer Consume for queue {0}, consumer tag is {1}", _queueName, _consumerTag);
			}
		}
	}
}
