using NUnit.Framework;
using PMCG.Messaging.Client.Configuration;
using PMCG.Messaging.Client.UT.TestDoubles;
using System;
using System.Linq;


namespace PMCG.Messaging.Client.UT.Configuration
{
	[TestFixture]
	public class MessagePublication
	{
		[Test]
		public void Ctor_Where_Event_With_Multiple_Message_Deliveries_Results_In_Object_Creation()
		{
			var _SUT = new PMCG.Messaging.Client.Configuration.MessagePublication(
				typeof(MyEvent),
				new []
					{
						new PMCG.Messaging.Client.Configuration.MessageDelivery(
							"exchangeName1",
							"typeHeader",
							MessageDeliveryMode.Persistent,
							message => string.Empty),
						new PMCG.Messaging.Client.Configuration.MessageDelivery(
							"exchangeName2",
							"typeHeader",
							MessageDeliveryMode.Persistent,
							message => string.Empty)
					});

			Assert.AreEqual(2, _SUT.Configurations.Count());
		}


		[Test]
		public void Ctor_Where_Command_With_Single_Message_Delivery_Results_In_Object_Creation()
		{
			var _SUT = new PMCG.Messaging.Client.Configuration.MessagePublication(
				typeof(MyCommand),
				new []
					{
						new PMCG.Messaging.Client.Configuration.MessageDelivery(
							"exchangeName1",
							"typeHeader",
							MessageDeliveryMode.Persistent,
							message => string.Empty)
					});

			Assert.AreEqual(1, _SUT.Configurations.Count());
		}


		[Test]
		public void Ctor_Where_Command_With_Multiple_Message_Deliveries_Results_In_An_Exception()
		{
			Assert.That(() => {
				new PMCG.Messaging.Client.Configuration.MessagePublication(
				typeof(MyCommand),
				new []
					{
						new PMCG.Messaging.Client.Configuration.MessageDelivery(
							"exchangeName1",
							"typeHeader",
							MessageDeliveryMode.Persistent,
							message => string.Empty),
						new PMCG.Messaging.Client.Configuration.MessageDelivery(
							"exchangeName2",
							"typeHeader",
							MessageDeliveryMode.Persistent,
							message => string.Empty)
					}); },
				Throws.TypeOf<ApplicationException>());
		}
	}
}