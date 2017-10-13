using System;


namespace PMCG.Messaging.Client.AT.Accessories
{
	public class Configuration
	{
		public const string ConnectionSettingsString = "hosts=localhost,anotherhost;port=5672;virtualhost=/;username=guest";
		public const string ConnectionClientProvidedName = "myTestConnection";
		public const string ExchangeName1 = "test.exchange.1";
		public const string QueueName1 = "test.queue.1";
		public const string ExchangeName2 = "test.exchange.2";
		public const string QueueName2 = "test.queue.2";
	}
}
