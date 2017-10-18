using System;
using System.Collections.Generic;


namespace PMCG.Messaging.Client.Configuration
{
	public class BusConfiguration
	{
		public readonly ConnectionSettings ConnectionSettings;
		public readonly TimeSpan ReconnectionPauseInterval;
		public readonly ushort NumberOfPublishers;
		public readonly ushort NumberOfConsumers;
		public readonly ushort ConsumerMessagePrefetchCount;
		public readonly TimeSpan ConsumerDequeueTimeout;
		public readonly MessagePublications MessagePublications;
		public readonly MessageConsumers MessageConsumers;


		public BusConfiguration(
			ConnectionSettings connectionSettings,
			TimeSpan reconnectionPauseInterval,
			ushort numberOfPublishers,
			ushort numberOfConsumers,
			ushort consumerMessagePrefetchCount,
			TimeSpan consumerDequeueTimeout,
			MessagePublications messagePublications,
			MessageConsumers messageConsumers)
		{
			Check.RequireArgumentNotNull("connectionSettings", connectionSettings);
			Check.RequireArgument("reconnectionPauseInterval", reconnectionPauseInterval, reconnectionPauseInterval.TotalSeconds > 0);
			Check.RequireArgument("numberOfPublishers", numberOfPublishers, numberOfPublishers > 0);
			Check.RequireArgument("numberOfConsumers", numberOfConsumers, numberOfConsumers > 0);
			Check.RequireArgument("consumerMessagePrefetchCount", consumerMessagePrefetchCount, consumerMessagePrefetchCount > 0);
			Check.RequireArgument("consumerDequeueTimeout", consumerDequeueTimeout, consumerDequeueTimeout.TotalSeconds > 0);
			Check.RequireArgumentNotNull("messagePublications", messagePublications);
			Check.RequireArgumentNotNull("messageConsumers", messageConsumers);
			
			this.ConnectionSettings = connectionSettings;
			this.ReconnectionPauseInterval = reconnectionPauseInterval;
			this.NumberOfPublishers = numberOfPublishers;
			this.NumberOfConsumers = numberOfConsumers;
			this.ConsumerMessagePrefetchCount = consumerMessagePrefetchCount;
			this.ConsumerDequeueTimeout = consumerDequeueTimeout;
			this.MessagePublications = messagePublications;
			this.MessageConsumers = messageConsumers;
		}
	}
}