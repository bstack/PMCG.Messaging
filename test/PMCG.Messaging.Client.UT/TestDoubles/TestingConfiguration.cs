using System;


namespace PMCG.Messaging.Client.UT.TestDoubles
{
	public class TestingConfiguration
	{
		public const string LocalConnectionUri = "amqp://guest:guest@localhost:5672/";
		public const string ConnectionClientProvidedName = "myConnectionClientProvidedName";
		public const string ExchangeName = "test.exchange.1";
		public const string QueueName = "test.queue.1";
	}
}
