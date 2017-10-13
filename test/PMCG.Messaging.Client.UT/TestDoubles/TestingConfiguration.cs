using System;


namespace PMCG.Messaging.Client.UT.TestDoubles
{
	public class TestingConfiguration
	{
		public const string ConnectionSettingsString = "hosts=localhost;port=5672;virtualhost=/;username=guest";
		public const string ConnectionClientProvidedName = "myConnectionClientProvidedName";
		public const string ExchangeName = "test.exchange.1";
		public const string QueueName = "test.queue.1";
	}
}
