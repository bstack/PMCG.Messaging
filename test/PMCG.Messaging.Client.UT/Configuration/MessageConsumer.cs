﻿using NUnit.Framework;
using PMCG.Messaging.Client.UT.TestDoubles;
using System;


namespace PMCG.Messaging.Client.UT.Configuration
{
	[TestFixture]
	public class MessageConsumer
	{
		private PMCG.Messaging.Client.Configuration.MessageConsumer c_SUT;


		[Test]
		public void Ctor_Where_Good_Params_Results_In_Object_Creation()
		{
			this.c_SUT = new PMCG.Messaging.Client.Configuration.MessageConsumer(
				typeof(MyEvent),
				TestingConfiguration.QueueName,
				typeof(MyEvent).Name,
				message => ConsumerHandlerResult.Completed);

			Assert.IsNotNull(this.c_SUT);
			Assert.AreEqual(typeof(MyEvent), this.c_SUT.Type);
			Assert.AreEqual(TestingConfiguration.QueueName, this.c_SUT.QueueName);
			Assert.AreEqual(typeof (MyEvent).Name, this.c_SUT.TypeHeader);
		}


		[Test]
		public void Ctor_Where_Type_Is_Not_A_Message_Results_In_An_Exception()
		{
			Assert.That(() => {
				this.c_SUT = new PMCG.Messaging.Client.Configuration.MessageConsumer(
					this.GetType(),
					TestingConfiguration.QueueName,
					typeof(MyEvent).Name,
					message => ConsumerHandlerResult.Completed);}, Throws.TypeOf<ArgumentException>());
		}
	}
}