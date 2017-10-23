using System;


namespace PMCG.Messaging.Client.UT
{
	public class Consumer
	{
		// NOTE: We dont have any UTs here as this class is fully dependent on RabbitMQ library concepts. There is hardly any logic that is worth testing here, 
		// verification of the behaviour of this class in conjunction with the consumer message processor should be done at an acceptence test level.
		// It would also be very difficult to test in any case as all RabbitMQ concepts would have to be abstracted and then injected into the Consumer class - 
		// bang for your buck here too low
	}
}
