﻿//using NSubstitute;
//using NUnit.Framework;
//using PMCG.Messaging.Client.BusState;
//using PMCG.Messaging.Client.Configuration;
//using RabbitMQ.Client;
//using RabbitMQ.Client.Events;
//using System;
//using System.Threading;


//namespace PMCG.Messaging.Client.UT.BusState
//{
//	[TestFixture]
//	public class ConnectedSpec
//	{
//		[Test]
//		public void Ctor_Success()
//		{
//			var _busConfigurationBuilder = new BusConfigurationBuilder();
//			_busConfigurationBuilder.ConnectionUris.Add(TestingConfiguration.LocalConnectionUri);
//			_busConfigurationBuilder.ConnectionClientProvidedName = TestingConfiguration.ConnectionClientProvidedName;
//			_busConfigurationBuilder
//				.RegisterPublication<MyEvent>(
//					TestingConfiguration.ExchangeName,
//					typeof(MyEvent).Name);
//			var _busConfirguration = _busConfigurationBuilder.Build();

//			var _connectionManager = Substitute.For<IConnectionManager>();
//			var _context = Substitute.For<IBusContext>();

//			new Connected(
//				_busConfirguration,
//				_connectionManager,
//				_context);
//		}


//		[Test]
//		public void PublishAsync_Where_No_Publication_Configurations_Which_Results_In_A_NoConfigurationFound_Result()
//		{
//			var _busConfigurationBuilder = new BusConfigurationBuilder();
//			_busConfigurationBuilder.ConnectionUris.Add(TestingConfiguration.LocalConnectionUri);
//			_busConfigurationBuilder.ConnectionClientProvidedName = TestingConfiguration.ConnectionClientProvidedName;
//			var _busConfirguration = _busConfigurationBuilder.Build();

//			var _connectionManager = Substitute.For<IConnectionManager>();
//			var _context = Substitute.For<IBusContext>();

//			var _SUT = new Connected(
//				_busConfirguration,
//				_connectionManager,
//				_context);

//			var _theEvent = new MyEvent(Guid.NewGuid(), null, "Some detail", 1);
//			var _publicationResult = _SUT.PublishAsync(_theEvent);
//			_publicationResult.Wait();

//			Assert.AreEqual(PMCG.Messaging.PublicationResultStatus.NoConfigurationFound, _publicationResult.Result.Status);
//		}

	
//		[Test]
//		public void PublishAsync_Where_A_Single_Publication_Configuration_Which_Results_In_A_Successfull_Publication()
//		{
//			var _busConfigurationBuilder = new BusConfigurationBuilder();
//			_busConfigurationBuilder.ConnectionUris.Add(TestingConfiguration.LocalConnectionUri);
//			_busConfigurationBuilder.ConnectionClientProvidedName = TestingConfiguration.ConnectionClientProvidedName;
//			_busConfigurationBuilder
//				.RegisterPublication<MyEvent>(
//					TestingConfiguration.ExchangeName,
//					typeof(MyEvent).Name);
//			var _busConfirguration = _busConfigurationBuilder.Build();

//			var _connectionManager = Substitute.For<IConnectionManager>();
//			var _connection = Substitute.For<IConnection>();
//			var _channel = Substitute.For<IModel>();
//			var _context = Substitute.For<IBusContext>();
//			var _waitHandle = new AutoResetEvent(false);

//			_connectionManager.Connection.Returns(_connection);
//			_connection.CreateModel().Returns(_channel);
//			_channel.NextPublishSeqNo.Returns(1UL);
//			_channel
//				.When(channel => channel.BasicPublish(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IBasicProperties>(), Arg.Any<byte[]>()))
//				.Do(callInfo => _waitHandle.Set());

//			var _SUT = new Connected(
//				_busConfirguration,
//				_connectionManager,
//				_context);

//			var _theEvent = new MyEvent(Guid.NewGuid(), null, "Some detail", 1);
//			var _publicationResult = _SUT.PublishAsync(_theEvent);

//			_waitHandle.WaitOne();	// Allow publication to complete
//			_channel.BasicAcks += Raise.EventWith(_channel, new BasicAckEventArgs { Multiple = true, DeliveryTag = 10 });
//			_publicationResult.Wait();

//			Assert.AreEqual(PMCG.Messaging.PublicationResultStatus.Published, _publicationResult.Result.Status);
//		}


//		[Test]
//		public void PublishAsync_Where_Multiple_Publication_Configurations_Which_Results_In_A_Successfull_Publication()
//		{
//			var _busConfigurationBuilder = new BusConfigurationBuilder();
//			_busConfigurationBuilder.ConnectionUris.Add(TestingConfiguration.LocalConnectionUri);
//			_busConfigurationBuilder.ConnectionClientProvidedName = TestingConfiguration.ConnectionClientProvidedName;
//			_busConfigurationBuilder
//				.RegisterPublication<MyEvent>(
//					TestingConfiguration.ExchangeName,
//					typeof(MyEvent).Name)
//				.RegisterPublication<MyEvent>(
//					TestingConfiguration.ExchangeName,
//					typeof(MyEvent).Name);
//			var _busConfirguration = _busConfigurationBuilder.Build();

//			var _connectionManager = Substitute.For<IConnectionManager>();
//			var _connection = Substitute.For<IConnection>();
//			var _channel = Substitute.For<IModel>();
//			var _context = Substitute.For<IBusContext>();
//			var _waitHandle = new CountdownEvent(2);

//			_connectionManager.Connection.Returns(_connection);
//			_connection.CreateModel().Returns(_channel);
//			_channel.NextPublishSeqNo.Returns(1UL, 2UL);		// Not sure why it works here, see next method comment
//			_channel
//				.When(channel => channel.BasicPublish(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IBasicProperties>(), Arg.Any<byte[]>()))
//				.Do(callInfo => _waitHandle.Signal());

//			var _SUT = new Connected(
//				_busConfirguration,
//				_connectionManager,
//				_context);

//			var _theEvent = new MyEvent(Guid.NewGuid(), null, "Some detail", 1);
//			var _publicationResult = _SUT.PublishAsync(_theEvent);

//			_waitHandle.Wait();	// Allow publication to complete
//			_channel.BasicAcks += Raise.EventWith(_channel, new BasicAckEventArgs { Multiple = true, DeliveryTag = 2 });
//			_publicationResult.Wait();

//			Assert.AreEqual(PMCG.Messaging.PublicationResultStatus.Published, _publicationResult.Result.Status);
//		}


//		[Test]
//		public void PublishAsync_Where_Multiple_Publication_Configurations_One_Of_Which_Is_Nacked_Results_In_Unsuccessfull_Publication()
//		{
//			var _busConfigurationBuilder = new BusConfigurationBuilder();
//			_busConfigurationBuilder.ConnectionUris.Add(TestingConfiguration.LocalConnectionUri);
//			_busConfigurationBuilder.ConnectionClientProvidedName = TestingConfiguration.ConnectionClientProvidedName;
//			_busConfigurationBuilder
//				.RegisterPublication<MyEvent>(
//					TestingConfiguration.ExchangeName,
//					typeof(MyEvent).Name)
//				.RegisterPublication<MyEvent>(
//					TestingConfiguration.ExchangeName,
//					typeof(MyEvent).Name)
//				.RegisterPublication<MyEvent>(
//					TestingConfiguration.ExchangeName,
//					typeof(MyEvent).Name);
//			var _busConfirguration = _busConfigurationBuilder.Build();

//			var _connectionManager = Substitute.For<IConnectionManager>();
//			var _connection = Substitute.For<IConnection>();
//			var _channel = Substitute.For<IModel>();
//			var _context = Substitute.For<IBusContext>();
//			var _waitHandle = new CountdownEvent(3);

//			_connectionManager.Connection.Returns(_connection);
//			_connection.CreateModel().Returns(_channel);
//			var _nextPublishSeqNo = 1UL;
//			_channel.NextPublishSeqNo.Returns(callInfo => _nextPublishSeqNo++);		// Would not work when I used .Returns(1Ul, 2UL, 3UL); Not sure why this works !
//			_channel
//				.When(channel => channel.BasicPublish(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IBasicProperties>(), Arg.Any<byte[]>()))
//				.Do(callInfo => _waitHandle.Signal());

//			var _SUT = new Connected(
//				_busConfirguration,
//				_connectionManager,
//				_context);

//			var _theEvent = new MyEvent(Guid.NewGuid(), null, "Some detail", 1);
//			var _publicationResult = _SUT.PublishAsync(_theEvent);

//			_waitHandle.Wait();			// Wait for publications to complete
//			_channel.BasicAcks += Raise.EventWith(_channel, new BasicAckEventArgs { Multiple = true, DeliveryTag = 2 });
//			_channel.BasicNacks += Raise.EventWith(_channel, new BasicNackEventArgs { Multiple = false, DeliveryTag = 3 });
//			_publicationResult.Wait();

//			Assert.AreEqual(PMCG.Messaging.PublicationResultStatus.NotPublished, _publicationResult.Result.Status);
//		}


//		[Test]
//		public void Close_Where_No_Pending_Publications_Which_Results_In_A_Context_Transition_To_Closed()
//		{
//			var _busConfigurationBuilder = new BusConfigurationBuilder();
//			_busConfigurationBuilder.ConnectionUris.Add(TestingConfiguration.LocalConnectionUri);
//			_busConfigurationBuilder.ConnectionClientProvidedName = TestingConfiguration.ConnectionClientProvidedName;
//			_busConfigurationBuilder
//				.RegisterPublication<MyEvent>(
//					TestingConfiguration.ExchangeName,
//					typeof(MyEvent).Name);
//			var _busConfirguration = _busConfigurationBuilder.Build();

//			var _connectionManager = Substitute.For<IConnectionManager>();
//			var _connection = Substitute.For<IConnection>();
//			var _channel = Substitute.For<IModel>();
//			var _context = Substitute.For<IBusContext>();

//			_connectionManager.Connection.Returns(_connection);
//			_connection.CreateModel().Returns(_channel);

//			var _SUT = new Connected(
//				_busConfirguration,
//				_connectionManager,
//				_context);

//			_context.State.Returns(callInfo => _SUT);
//			State _capturedState = null;
//			_context.When(context => context.State = Arg.Any<State>()).Do(callInfo => _capturedState = callInfo[0] as State);
//			_SUT.Close();

//			Assert.IsInstanceOf<Closed>(_capturedState);
//		}


//		[Test]
//		public void State_Changed_Where_Connection_Is_Blocked_Results_In_A_Context_Transition_To_Blocked()
//		{
//			var _busConfigurationBuilder = new BusConfigurationBuilder();
//			_busConfigurationBuilder.ConnectionUris.Add(TestingConfiguration.LocalConnectionUri);
//			_busConfigurationBuilder.ConnectionClientProvidedName = TestingConfiguration.ConnectionClientProvidedName;
//			_busConfigurationBuilder
//				.RegisterPublication<MyEvent>(
//					TestingConfiguration.ExchangeName,
//					typeof(MyEvent).Name);
//			var _busConfirguration = _busConfigurationBuilder.Build();

//			var _connectionManager = Substitute.For<IConnectionManager>();
//			var _connection = Substitute.For<IConnection>();
//			var _channel = Substitute.For<IModel>();
//			var _context = Substitute.For<IBusContext>();

//			_connectionManager.Connection.Returns(_connection);
//			_connection.CreateModel().Returns(_channel);

//			var _SUT = new Connected(
//				_busConfirguration,
//				_connectionManager,
//				_context);

//			_context.State.Returns(callInfo => _SUT);
//			State _capturedState = null;
//			_context.When(context => context.State = Arg.Any<State>()).Do(callInfo => _capturedState = callInfo[0] as State);
//			_connectionManager.Blocked += Raise.Event<EventHandler<ConnectionBlockedEventArgs>>(_connection, new ConnectionBlockedEventArgs("."));

//			Assert.IsInstanceOf<Blocked>(_capturedState);
//		}


//		[Test]
//		public void State_Changed_Where_Connection_Is_Disconnected_Results_In_A_Context_Transition_To_Diconnected()
//		{
//			var _busConfigurationBuilder = new BusConfigurationBuilder();
//			_busConfigurationBuilder.ConnectionUris.Add(TestingConfiguration.LocalConnectionUri);
//			_busConfigurationBuilder.ConnectionClientProvidedName = TestingConfiguration.ConnectionClientProvidedName;
//			_busConfigurationBuilder
//				.RegisterPublication<MyEvent>(
//					TestingConfiguration.ExchangeName,
//					typeof(MyEvent).Name);
//			var _busConfirguration = _busConfigurationBuilder.Build();

//			var _connectionManager = Substitute.For<IConnectionManager>();
//			var _connection = Substitute.For<IConnection>();
//			var _channel = Substitute.For<IModel>();
//			var _context = Substitute.For<IBusContext>();

//			_connectionManager.Connection.Returns(_connection);
//			_connection.CreateModel().Returns(_channel);

//			var _SUT = new Connected(
//				_busConfirguration,
//				_connectionManager,
//				_context);

//			_context.State.Returns(callInfo => _SUT);
//			State _capturedState = null;
//			_context.When(context => context.State = Arg.Any<State>()).Do(callInfo => _capturedState = callInfo[0] as State);
//			_connectionManager.Disconnected += Raise.Event<EventHandler<ConnectionDisconnectedEventArgs>>(_connection, new ConnectionDisconnectedEventArgs(1, "."));

//			Assert.IsInstanceOf<Disconnected>(_capturedState);
//		}


//		[Test]
//		public void PublishAsync_Where_Connection_Is_Disconnected_Results_In_A_Non_Published_Result()
//		{
//			var _busConfigurationBuilder = new BusConfigurationBuilder();
//			_busConfigurationBuilder.ConnectionUris.Add(TestingConfiguration.LocalConnectionUri);
//			_busConfigurationBuilder.ConnectionClientProvidedName = TestingConfiguration.ConnectionClientProvidedName;
//			_busConfigurationBuilder
//				.RegisterPublication<MyEvent>(
//					TestingConfiguration.ExchangeName,
//					typeof(MyEvent).Name);
//			var _busConfirguration = _busConfigurationBuilder.Build();

//			var _connectionManager = Substitute.For<IConnectionManager>();
//			var _connection = Substitute.For<IConnection>();
//			var _channel = Substitute.For<IModel>();
//			var _context = Substitute.For<IBusContext>();

//			_connectionManager.Connection.Returns(_connection);
//			_connection.CreateModel().Returns(_channel);

//			var _SUT = new Connected(
//				_busConfirguration,
//				_connectionManager,
//				_context);

//			_context.State.Returns(callInfo => _SUT);
//			_connectionManager.Disconnected += Raise.Event<EventHandler<ConnectionDisconnectedEventArgs>>(_connection, new ConnectionDisconnectedEventArgs(1, "."));

//			var _theEvent = new MyEvent(Guid.NewGuid(), null, "Some detail", 1);
//			var _publicationResult = _SUT.PublishAsync(_theEvent);
//			_publicationResult.Wait();

//			Assert.AreEqual(PMCG.Messaging.PublicationResultStatus.NotPublished, _publicationResult.Result.Status);
//		}


//		[Test]
//		public void State_Changed_Where_Connection_Is_Disconnected_And_A_Pending_Publication_Results_In_Pending_Publication_Task_Completion()
//		{
//			var _busConfigurationBuilder = new BusConfigurationBuilder();
//			_busConfigurationBuilder.ConnectionUris.Add(TestingConfiguration.LocalConnectionUri);
//			_busConfigurationBuilder.ConnectionClientProvidedName = TestingConfiguration.ConnectionClientProvidedName;
//			_busConfigurationBuilder.NumberOfPublishers = 1;	// Only one publisher
//			_busConfigurationBuilder
//				.RegisterPublication<MyEvent>(
//					TestingConfiguration.ExchangeName,
//					typeof(MyEvent).Name);
//			var _busConfirguration = _busConfigurationBuilder.Build();

//			var _connectionManager = Substitute.For<IConnectionManager>();
//			var _connection = Substitute.For<IConnection>();
//			var _channel = Substitute.For<IModel>();
//			var _context = Substitute.For<IBusContext>();

//			_connectionManager.Connection.Returns(_connection);
//			_connection.CreateModel().Returns(_channel);
//			_channel.NextPublishSeqNo.Returns(1UL, 2UL);
//			_channel
//				.When(channel => channel.BasicPublish(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IBasicProperties>(), Arg.Any<byte[]>()))
//				.Do(callInfo => { Thread.Sleep(1000); });		// Slow down so second publication will not get a chance to be published as we have only one publisher - therefore only one queue consumer

//			var _SUT = new Connected(
//				_busConfirguration,
//				_connectionManager,
//				_context);

//			var _theEvent = new MyEvent(Guid.NewGuid(), null, "Some detail", 1);
//			var _publicationResult1 = _SUT.PublishAsync(_theEvent);
//			var _publicationResult2 = _SUT.PublishAsync(_theEvent);

//			_context.State.Returns(callInfo => _SUT);
//			State _capturedState = null;
//			_context.When(context => context.State = Arg.Any<State>()).Do(callInfo => _capturedState = callInfo[0] as State);
//			_connectionManager.Disconnected += Raise.Event<EventHandler<ConnectionDisconnectedEventArgs>>(_connection, new ConnectionDisconnectedEventArgs(1, "."));

//			Assert.IsNotNull(_publicationResult1);
//			Assert.IsNotNull(_capturedState);
			
//			_publicationResult2.Wait();		// Second never gets pulled from queue due to slow BasicPublish on first message
//			Assert.AreEqual(PMCG.Messaging.PublicationResultStatus.NotPublished, _publicationResult2.Result.Status);
//		}
//	}
//}
