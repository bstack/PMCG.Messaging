using PMCG.Messaging.Client.Configuration;
using System;
using System.Collections.Concurrent;


namespace PMCG.Messaging.Client
{
	public class BusPublishersConsumersSeam : IBusPublishersConsumersSeam
	{
		public void CreateConsumers(
			BusConfiguration configuration,
			IConnectionManager connectionManager,
			BlockingCollection<Publication> publicationQueue)
		{
			for (var _index = 0; _index < configuration.NumberOfConsumers; _index++)
			{
				new Consumer(connectionManager.Connection, configuration).Start();
			}
		}


		public void CreatePublishers(
			BusConfiguration configuration,
			IConnectionManager connectionManager,
			BlockingCollection<Publication> publicationQueue)
		{
			for (var _index = 0; _index < configuration.NumberOfPublishers; _index++)
			{
				new Publisher(connectionManager.Connection, publicationQueue).Start();
			}
		}
	}
}