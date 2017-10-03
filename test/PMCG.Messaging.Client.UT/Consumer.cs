using NSubstitute;
using NUnit.Framework;
using PMCG.Messaging.Client.Configuration;
using PMCG.Messaging.Client.UT.TestDoubles;
using RabbitMQ.Client;
using System;


namespace PMCG.Messaging.Client.UT
{
	[TestFixture]
	public class Consumer
	{
		private BusConfiguration c_busConfiguration;
		private IConnection c_connection;
		private IModel c_channel;


		[SetUp]
		public void SetUp()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(TestingConfiguration.LocalConnectionUri);
			_busConfigurationBuilder.ConnectionClientProvidedName = TestingConfiguration.ConnectionClientProvidedName;
			_busConfigurationBuilder.ConsumerDequeueTimeout = TimeSpan.FromMilliseconds(20);
			_busConfigurationBuilder.RegisterConsumer<MyEvent>(
				TestingConfiguration.QueueName,
				typeof(MyEvent).Name,
				message =>
					{
						return ConsumerHandlerResult.Completed;
					});
			this.c_busConfiguration = _busConfigurationBuilder.Build();

			this.c_connection = Substitute.For<IConnection>();
			this.c_channel = Substitute.For<IModel>();

			this.c_connection.CreateModel().Returns(this.c_channel);
		}


		[Test]
		public void Start_Where_Successful()
		{
			var _SUT = new PMCG.Messaging.Client.Consumer(
				this.c_connection,
				this.c_busConfiguration);

			_SUT.Start();
		}


		// TODO: BS Pending ...
		//[Test]
		//public void Consume_Where_We_Mock_All_Without_A_Real_Connection_Knows_Too_Much_About_RabbitMQ_Internals()
		//{
		//	var _waitHandle = new AutoResetEvent(false);
		//	var _capturedMessageId = Guid.Empty;

		//	var _configurationBuilder = new BusConfigurationBuilder();
		//	_configurationBuilder.ConnectionUris.Add(TestingConfiguration.LocalConnectionUri);
		//	_configurationBuilder.RegisterConsumer<MyEvent>(
		//		TestingConfiguration.QueueName,
		//		typeof(MyEvent).Name,
		//		message =>
		//			{
		//				_capturedMessageId = message.Id;
		//				_waitHandle.Set();
		//				return ConsumerHandlerResult.Completed;
		//			});
		//	var _configuration = _configurationBuilder.Build();

		//	var _connection = Substitute.For<IConnection>();
		//	var _channel = Substitute.For<IModel>();
			
		//	_connection.CreateModel().Returns(_channel);
	
		//	var _myEvent = new MyEvent(Guid.NewGuid(), "CorrlationId_1", "Detail", 1);
		//	var _messageProperties = Substitute.For<IBasicProperties>();
		//	_messageProperties.ContentType = "application/json";
		//	_messageProperties.DeliveryMode = (byte)MessageDeliveryMode.Persistent;
		//	_messageProperties.Type = typeof(MyEvent).Name;
		//	_messageProperties.MessageId = _myEvent.Id.ToString();
		//	_messageProperties.CorrelationId = _myEvent.CorrelationId;
		//	_channel.CreateBasicProperties().Returns(_messageProperties);

		//	EventingBasicConsumer _capturedConsumer = null;
		//	_channel
		//		.When(channel => channel.BasicConsume(TestingConfiguration.QueueName, false, Arg.Any<IBasicConsumer>()))
		//		.Do(callInfo => { _capturedConsumer = callInfo[2] as EventingBasicConsumer; _waitHandle.Set(); });

		//	var _SUT = new Consumer(_connection, _configuration, CancellationToken.None);
		//	_SUT.Start();				// Can't capture result due to compiler treating warnings as errors 
		//	_waitHandle.WaitOne();			// Wait till consumer task has called the BasicConsume method which captures the consumer
		//	_waitHandle.Reset();			// Reset so we can block on the consumer message func

		//	var _messageJson = JsonConvert.SerializeObject(_myEvent);
		//	var _messageBody = Encoding.UTF8.GetBytes(_messageJson);
		//	_capturedConsumer.Queue.Enqueue(
		//		new BasicDeliverEventArgs
		//			{
		//				ConsumerTag = "consumerTag",
		//				DeliveryTag = 1UL,
		//				Redelivered = false,
		//				Exchange = "TheExchange",
		//				RoutingKey = "ARoutingKey",
		//				BasicProperties = _messageProperties,
		//				Body = _messageBody
		//			});
		//	_waitHandle.WaitOne();		// Wait for message to be consumed

		//	Assert.AreEqual(_myEvent.Id, _capturedMessageId);
		//}
	}
}
