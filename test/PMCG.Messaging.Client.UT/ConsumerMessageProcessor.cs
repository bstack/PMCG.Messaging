using NSubstitute;
using NUnit.Framework;
using PMCG.Messaging.Client.Configuration;
using PMCG.Messaging.Client.UT.TestDoubles;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;


namespace PMCG.Messaging.Client.UT
{
	[TestFixture]
	public class ConsumerMessageProcessor
	{
		private IModel c_channel;


		[SetUp]
		public void SetUp()
		{
			var _connection = Substitute.For<IConnection>();

			this.c_channel = Substitute.For<IModel>();
			_connection.CreateModel().Returns(this.c_channel);
		}


		[Test]
		public void Process_Where_Unknown_Type_Results_In_Channel_Being_Nacked()
		{
			var _SUT = this.BuildSUT();
			var _message = this.BuildBasicDeliverEventArgs("Unknown");

			_SUT.Process(this.c_channel, _message);

			this.c_channel.Received().BasicNack(_message.DeliveryTag, false, false);
		}


		[Test]
		public void Process_Where_Known_Type_Results_In_Channel_Being_Acked()
		{
			var _SUT = this.BuildSUT(message => ConsumerHandlerResult.Completed);
			var _message = this.BuildBasicDeliverEventArgs(typeof(MyEvent).Name);

			_SUT.Process(this.c_channel, _message);

			this.c_channel.Received().BasicAck(_message.DeliveryTag, false);
		}


		[Test]
		public void Process_Where_Message_Action_Throws_Exception_Results_In_Channel_Being_Nacked()
		{
			var _SUT = this.BuildSUT(message => throw new Exception("BANG!"));
			var _message = this.BuildBasicDeliverEventArgs("Throw_Error_Type_Header");

			_SUT.Process(this.c_channel, _message);

			this.c_channel.Received().BasicNack(_message.DeliveryTag, false, false);
		}

	
		[Test]
		public void Process_Where_Message_Action_Returns_Errored_Results_In_Channel_Being_Nacked()
		{
			var _SUT = this.BuildSUT(message => ConsumerHandlerResult.Errored);
			var _message = this.BuildBasicDeliverEventArgs(typeof(MyEvent).Name);

			_SUT.Process(this.c_channel, _message);

			this.c_channel.Received().BasicNack(_message.DeliveryTag, false, false);
		}


		[Test]
		public void Process_Where_Message_Action_Returns_Requeue_Results_In_Channel_Being_Rejected()
		{
			var _SUT = this.BuildSUT(message => ConsumerHandlerResult.Requeue);
			var _message = this.BuildBasicDeliverEventArgs(typeof(MyEvent).Name);

			_SUT.Process(this.c_channel, _message);

			this.c_channel.Received().BasicReject(_message.DeliveryTag, true);
		}


		private PMCG.Messaging.Client.ConsumerMessageProcessor BuildSUT(
			Func<MyEvent, ConsumerHandlerResult> action = null)
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder("hosts=localhost;port=5672;virtualhost=/;username=guest;ispasswordencrypted=false;password=Pass");
			_busConfigurationBuilder.ConnectionClientProvidedName = TestingConfiguration.ConnectionClientProvidedName;
			_busConfigurationBuilder.RegisterConsumer(
				TestingConfiguration.QueueName,
				typeof(MyEvent).Name,
				action);
			var _busConfiguration = _busConfigurationBuilder.Build();

			return new PMCG.Messaging.Client.ConsumerMessageProcessor(_busConfiguration);
		}


		private BasicDeliverEventArgs BuildBasicDeliverEventArgs(
			string typeHeader)
		{
			var _properties = Substitute.For<IBasicProperties>();
			_properties.Type.Returns(typeHeader);

			return new BasicDeliverEventArgs(
				consumerTag: Guid.NewGuid().ToString(),
				deliveryTag: 1L,
				redelivered: false,
				exchange: "TheExchangeName",
				routingKey: "RoutingKey",
				properties: _properties,
				body: new byte[0]);
		}
	}
}
