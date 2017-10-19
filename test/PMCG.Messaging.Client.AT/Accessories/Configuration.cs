using System;


namespace PMCG.Messaging.Client.AT.Accessories
{
	public class Configuration
	{
		public const string ConnectionSettingsString = "hosts=localhost;port=5672;clientprovidedname=myTestConnection;virtualhost=/;;username=appuser";
		//public const string ConnectionSettingsString = "hosts=trdevmq02a.ccs.local,trdevmq02b.ccs.local;port=5672;clientprovidedname=myTestConnection;virtualhost=/;;username=appuser";
		public const string ExchangeName1 = "test.exchange.1";
		public const string QueueName1 = "test.queue.1";
		public const string ExchangeName2 = "test.exchange.2";
		public const string QueueName2 = "test.queue.2";
	}
}
