using System;
using System.Collections.Concurrent;
using PMCG.Messaging.Client.Configuration;


namespace PMCG.Messaging.Client
{
	public interface IBusPublishersConsumersSeam
	{
		void CreatePublishers(
			BusConfiguration configuration,
			IConnectionManager connectionManager,
			BlockingCollection<Publication> publicationQueue);


		void CreateConsumers(
			BusConfiguration configuration,
			IConnectionManager connectionManager,
			BlockingCollection<Publication> publicationQueue);
	}
}
