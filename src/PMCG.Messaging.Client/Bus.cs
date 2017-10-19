using log4net;
using PMCG.Messaging.Client.Configuration;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;


namespace PMCG.Messaging.Client
{
	public class Bus : IBusController, IBus
	{
		private readonly ILog c_logger;
		private readonly BusConfiguration c_configuration;
		private readonly IBusPublishersConsumersSeam c_publishersConsumersSeam;
		private readonly IConnectionManager c_connectionManager;
		private readonly BlockingCollection<Publication> c_publicationQueue;


		public BlockingCollection<Publication> PublicationQueue { get { return this.c_publicationQueue; } }


		public Bus(
			BusConfiguration configuration) : this(configuration,
				new BusPublishersConsumersSeam(),
				new ConnectionManager(
					configuration.ConnectionSettings,
					configuration.ReconnectionPauseInterval))
		{
			// NOTE: We dont have a DBC check for BusConfiguration here as it does not execute before the call to the chained ctor. 
			// However there are ample DBC checks elsewhere to cover this and throw an exception which will surface on startup - this is behaviour which is desired
			// Lastly, we only want a single parameter i.e. bus configuration to be passed by the consumer of this library - need to keep usage as simple as possible
		}


		public Bus(
			BusConfiguration configuration,
			IBusPublishersConsumersSeam publishersConsumersSeam,
			IConnectionManager connectionManager)
		{
			this.c_logger = LogManager.GetLogger(this.GetType());
			this.c_logger.Info("ctor Starting");

			Check.RequireArgumentNotNull("configuration", configuration);
			Check.RequireArgumentNotNull("publishersConsumersSeam", publishersConsumersSeam);
			Check.RequireArgumentNotNull("connectionManager", connectionManager);

			this.c_configuration = configuration;
			this.c_publishersConsumersSeam = publishersConsumersSeam;
			this.c_connectionManager = connectionManager;
			this.c_publicationQueue = new BlockingCollection<Publication>();

			this.c_logger.Info("ctor Completed");
		}


		public void Connect()
		{
			this.c_logger.Info("Connect Starting");
			this.c_connectionManager.Open();

			if (!this.c_connectionManager.IsOpen)
			{
				this.c_logger.Error("Failed to connect to server, connection manager connection is closed");
				throw new ApplicationException("Failed to connect to server, connection manager connection is closed");
			}

			this.c_logger.Info("Connect About to create publisher tasks");
			this.c_publishersConsumersSeam.CreatePublishers(this.c_configuration, this.c_connectionManager, this.c_publicationQueue);

			this.c_logger.Info("Connect About to create consumers");
			this.c_publishersConsumersSeam.CreateConsumers(this.c_configuration, this.c_connectionManager, this.c_publicationQueue);

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
			Check.RequireArgumentNotNull("message", message);

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
							this.c_logger,
							deliveryConfiguration,
							message,
							new TaskCompletionSource<PublicationResult>()));

				var _tasks = new ConcurrentBag<Task<PublicationResult>>();
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