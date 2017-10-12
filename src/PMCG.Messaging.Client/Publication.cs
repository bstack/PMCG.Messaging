using log4net;
using PMCG.Messaging.Client.Configuration;
using System;
using System.Threading.Tasks;


namespace PMCG.Messaging.Client
{
	public class Publication
	{
		private readonly ILog c_logger;
		private readonly MessageDelivery c_configuration;
		private readonly Message c_message;
		private readonly TaskCompletionSource<PublicationResult> c_result;


		public string Id { get { return this.c_message.Id.ToString(); } }
		public string CorrelationId { get { return this.c_message.CorrelationId; } }
		public string ExchangeName { get { return this.c_configuration.ExchangeName; } }
		public Byte DeliveryMode { get { return (byte)this.c_configuration.DeliveryMode; } }
		public string RoutingKey { get { return this.c_configuration.RoutingKeyFunc(this.c_message); } }
		public string TypeHeader { get { return this.c_configuration.TypeHeader; } }
		public Task<PublicationResult> ResultTask { get { return this.c_result.Task; } }
		public Message Message { get { return this.c_message; } }


		public Publication(
			ILog logger,
			MessageDelivery configuration,
			Message message,
			TaskCompletionSource<PublicationResult> result)
		{
			this.c_logger = logger;
			this.c_configuration = configuration;
			this.c_message = message;
			this.c_result = result;
		}


		public void SetResult(
			PublicationResultStatus status,
			string context = null)
		{
			var _publicationResult = new PublicationResult(this.c_configuration, this.c_message, status, context);

			// We had to try setting the result here as we had a race condition issue as when a channel shuts down, we set all PublicationResults for all unconfirmed publications
			// that is less than the highest delivery tag value to ChannelShutdown. However at the same time, sometimes under high load we see the odd ChannelAck returning for the same
			// PublicationResult that has already been previously set to ChannelShutdown. Hence we have an edge case where we try to set the result for a Publication whose result has already 
			// been set. If this edge case occurs, we simply log that it occurred as it has no negative effect for the calling client anyway i.e. the calling client will have already received
			// a response back from the library due to ChannelShutdown first and will no longer be waiting.
			if(!this.c_result.TrySetResult(_publicationResult))
			{
				this.c_logger.Info("TrySetResult unsuccessful for PublicationResult. Likelihood is that Task was already set due to ChannelShutdown");
			}
		}
	}
}