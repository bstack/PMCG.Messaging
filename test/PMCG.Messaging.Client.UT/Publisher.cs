﻿using NSubstitute;
using NUnit.Framework;
using PMCG.Messaging.Client.Configuration;
using PMCG.Messaging.Client.UT.TestDoubles;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace PMCG.Messaging.Client.UT
{
	[TestFixture]
	public class Publisher
	{
		[Test]
		public void Publish_Failure_Unhandled_Exception_Task_Terminates()
		{
			var _connection = Substitute.For<IConnection>();
			var _channel = Substitute.For<IModel>();
			var _publicationQueue = new BlockingCollection<Publication>();

			_connection.CreateModel().Returns(_channel);
			_channel.CreateBasicProperties().Returns(callInfo => { throw new Exception("Channel not open !"); });

			var _messageDelivery = new MessageDelivery("test_publisher_confirms", typeof(MyEvent).Name, MessageDeliveryMode.Persistent, message => "ARoutingKey");
			var _myEvent = new MyEvent(Guid.NewGuid(), "CorrlationId_1", "Detail", 1);
			var _taskCompletionSource = new TaskCompletionSource<PublicationResult>();
			var _publication = new Publication(_messageDelivery, _myEvent, _taskCompletionSource);

			var _SUT = new PMCG.Messaging.Client.Publisher(_connection, _publicationQueue);
			var _publisherTask = _SUT.Start();

			_publicationQueue.Add(_publication);

			bool _isFaultedPublisherTask = false;
			try { _publisherTask.Wait(); } catch { _isFaultedPublisherTask = true; }

			Assert.IsTrue(_isFaultedPublisherTask);
		}


		[Test]
		public void Publish_Failure_Handled_Exception_Task_Continues()
		{
			var _connection = Substitute.For<IConnection>();
			var _channel = Substitute.For<IModel>();
			_channel.IsOpen.Returns(true);
			var _publicationQueue = new BlockingCollection<Publication>();

			_connection.CreateModel().Returns(_channel);
			_channel
				.When(channel => channel.BasicPublish(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IBasicProperties>(), Arg.Any<byte[]>()))
				.Do(callInfo => { throw new ApplicationException("Bang !"); });

			var _messageDelivery = new MessageDelivery("EXCHANGE", typeof(MyEvent).Name, MessageDeliveryMode.Persistent, message => "ARoutingKey");
			var _myEvent = new MyEvent(Guid.NewGuid(), "CorrlationId_1", "Detail", 1);
			var _taskCompletionSource = new TaskCompletionSource<PublicationResult>();
			var _publication = new Publication(_messageDelivery, _myEvent, _taskCompletionSource);

			var _SUT = new PMCG.Messaging.Client.Publisher(_connection, _publicationQueue);
			var _publisherTask = _SUT.Start();

			_publicationQueue.Add(_publication);

			// We wait 5 seconds to show that the task is still running and working - adding meesage back on to collection and trying to publish again in a continuous loop
			// Assert that the task has not completed as it is long lived.
			Thread.Sleep(TimeSpan.FromSeconds(5));
			Assert.IsFalse(_publication.ResultTask.IsCompleted);
		}



		[Test]
		public void Publish_Failure_Channel_Is_Closed_Task_Continues()
		{
			var _connection = Substitute.For<IConnection>();
			var _channel = Substitute.For<IModel>();
			_channel.IsOpen.Returns(false);
			var _publicationQueue = new BlockingCollection<Publication>();
			_connection.CreateModel().Returns(_channel);

			var _messageDelivery = new MessageDelivery("EXCHANGE", typeof(MyEvent).Name, MessageDeliveryMode.Persistent, message => "ARoutingKey");
			var _myEvent = new MyEvent(Guid.NewGuid(), "CorrlationId_1", "Detail", 1);
			var _taskCompletionSource = new TaskCompletionSource<PublicationResult>();
			var _publication = new Publication(_messageDelivery, _myEvent, _taskCompletionSource);

			var _SUT = new PMCG.Messaging.Client.Publisher(_connection, _publicationQueue);
			var _publisherTask = _SUT.Start();

			_publicationQueue.Add(_publication);

			// We wait 5 seconds to show that the task is still running and working - adding meesage back on to collection and trying to publish again in a continuous loop
			// Assert that the task has not completed as it is long lived.
			Thread.Sleep(TimeSpan.FromSeconds(5));
			Assert.IsFalse(_publication.ResultTask.IsCompleted);
		}


		[Test]
		public void Publish_Success_Task_Completes()
		{
			var _connection = Substitute.For<IConnection>();
			var _channel = Substitute.For<IModel>();
			_channel.IsOpen.Returns(true);
			var _publicationQueue = new BlockingCollection<Publication>();
			var _messageProperties = Substitute.For<IBasicProperties>();
			var _waitHandle = new AutoResetEvent(false);

			_connection.CreateModel().Returns(_channel);
			_channel.CreateBasicProperties().Returns(_messageProperties);
			_channel.NextPublishSeqNo.Returns(1UL);
			_channel
				.When(channel => channel.BasicPublish(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IBasicProperties>(), Arg.Any<byte[]>()))
				.Do(callInfo => _waitHandle.Set());

			var _SUT = new PMCG.Messaging.Client.Publisher(_connection, _publicationQueue);
			_SUT.Start(); 	// Can't capture result due to compiler treating warnings as errors - var is not used

			var _messageDelivery = new MessageDelivery("test_publisher_confirms", typeof(MyEvent).Name, MessageDeliveryMode.Persistent, message => "ARoutingKey");
			var _myEvent = new MyEvent(Guid.NewGuid(), "CorrlationId_1", "Detail", 1);
			var _taskCompletionSource = new TaskCompletionSource<PublicationResult>();
			var _publication = new Publication(_messageDelivery, _myEvent, _taskCompletionSource);
			_publicationQueue.Add(_publication);

			_waitHandle.WaitOne();	// Allow publication to complete
			_channel.BasicAcks += Raise.EventWith(_channel, new BasicAckEventArgs { Multiple = false, DeliveryTag = 1 });
			
			Assert.IsTrue(_publication.ResultTask.IsCompleted);
			_messageProperties.Received().ContentType = "application/json";
			_messageProperties.Received().DeliveryMode = (byte)_messageDelivery.DeliveryMode;
			_messageProperties.Received().Type = _messageDelivery.TypeHeader;
			_messageProperties.Received().MessageId = _myEvent.Id.ToString();
			_messageProperties.Received().CorrelationId = _myEvent.CorrelationId;
			_channel.Received().BasicPublish(_messageDelivery.ExchangeName, _messageDelivery.RoutingKeyFunc(_myEvent), _messageProperties, Arg.Any<byte[]>());
		}


		[Test]
		public void Publish_Success_Multiple_Publications_Task_Completes()
		{
			var _connection = Substitute.For<IConnection>();
			var _channel = Substitute.For<IModel>();
			_channel.IsOpen.Returns(true);
			var _publicationQueue = new BlockingCollection<Publication>();
			var _waitHandle = new CountdownEvent(10);

			_connection.CreateModel().Returns(_channel);
			var _nextPublishSeqNo = 1UL;
			_channel.NextPublishSeqNo.Returns(callInfo => _nextPublishSeqNo++);		// Would not work when I used .Returns(1Ul, 2UL, 3UL); Not sure why this works !
			_channel
				.When(channel => channel.BasicPublish(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IBasicProperties>(), Arg.Any<byte[]>()))
				.Do(callInfo => _waitHandle.Signal());

			var _SUT = new PMCG.Messaging.Client.Publisher(_connection, _publicationQueue);
			_SUT.Start();	// Can't capture due to compiler treating warnings as errors

			var _publications = new List<Publication>();
			for (var _index = 1; _index <= 10; _index++)
			{
				var _messageDelivery = new MessageDelivery("test_publisher_confirms", typeof(MyEvent).Name, MessageDeliveryMode.Persistent, message => "ARoutingKey");
				var _myEvent = new MyEvent(Guid.NewGuid(), "CorrlationId_1", "Detail", _index);
				var _taskCompletionSource = new TaskCompletionSource<PublicationResult>();
				var _publication = new Publication(_messageDelivery, _myEvent, _taskCompletionSource);

				_publicationQueue.Add(_publication);
				_publications.Add(_publication);
			}

			_waitHandle.Wait();	// Allow publications to complete
			foreach(var _publication in _publications) { Assert.IsFalse(_publication.ResultTask.IsCompleted); }
			_channel.BasicAcks += Raise.EventWith(_channel, new BasicAckEventArgs { Multiple = true, DeliveryTag = (ulong)_publications.Count });
			foreach(var _publication in _publications)
			{
				Assert.IsTrue(_publication.ResultTask.IsCompleted);
				Assert.AreEqual(PublicationResultStatus.Acked, _publication.ResultTask.Result.Status);
			}
		}


		[Test]
		public void Publish_Success_Multiple_Publications_Only_Some_Acked_Task_Continues()
		{
			var _connection = Substitute.For<IConnection>();
			var _channel = Substitute.For<IModel>();
			_channel.IsOpen.Returns(true);
			var _publicationQueue = new BlockingCollection<Publication>();
			var _waitHandle = new CountdownEvent(100);

			_connection.CreateModel().Returns(_channel);
			var _nextPublishSeqNo = 1UL;
			_channel.NextPublishSeqNo.Returns(callInfo => _nextPublishSeqNo++);		// Would not work when I used .Returns(1Ul, 2UL, 3UL); Not sure why this works !
			_channel
				.When(channel => channel.BasicPublish(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IBasicProperties>(), Arg.Any<byte[]>()))
				.Do(callInfo => _waitHandle.Signal());

			var _SUT = new PMCG.Messaging.Client.Publisher(_connection, _publicationQueue);
			_SUT.Start();

			var _publications = new List<Publication>();
			for (var _index = 1; _index <= 100; _index++)
			{
				var _messageDelivery = new MessageDelivery("test_publisher_confirms", typeof(MyEvent).Name, MessageDeliveryMode.Persistent, message => "ARoutingKey");
				var _myEvent = new MyEvent(Guid.NewGuid(), "CorrlationId_1", "Detail", _index);
				var _taskCompletionSource = new TaskCompletionSource<PublicationResult>();
				var _publication = new Publication(_messageDelivery, _myEvent, _taskCompletionSource);

				_publicationQueue.Add(_publication);
				_publications.Add(_publication);
			}

			_waitHandle.Wait();		// Allow channel publications to complete
			foreach(var _publication in _publications) { Assert.IsFalse(_publication.ResultTask.IsCompleted); }
			var _deliveryTagToAcknowledge = 73;
			_channel.BasicAcks += Raise.EventWith(_channel, new BasicAckEventArgs { Multiple = true, DeliveryTag = (ulong)_deliveryTagToAcknowledge });

			Assert.AreEqual(_deliveryTagToAcknowledge, _publications.Count(publication => publication.ResultTask.IsCompleted), "A1");
			Assert.AreEqual(_deliveryTagToAcknowledge, _publications.Count(publication => publication.ResultTask.IsCompleted && publication.ResultTask.Result.Status == PublicationResultStatus.Acked), "A2");
			Assert.AreEqual(100 - _deliveryTagToAcknowledge, _publications.Count(publication => !publication.ResultTask.IsCompleted), "A3");
		}


		[Test]
		public void Publish_Success_Nacked_Task_Completes()
		{
			var _connection = Substitute.For<IConnection>();
			var _channel = Substitute.For<IModel>();
			_channel.IsOpen.Returns(true);
			var _publicationQueue = new BlockingCollection<Publication>();
			var _waitHandle = new AutoResetEvent(false);

			_connection.CreateModel().Returns(_channel);
			_channel.NextPublishSeqNo.Returns(1Ul);
			_channel
				.When(channel => channel.BasicPublish(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IBasicProperties>(), Arg.Any<byte[]>()))
				.Do(callInfo => _waitHandle.Set());

			var _SUT = new PMCG.Messaging.Client.Publisher(_connection, _publicationQueue);
			_SUT.Start();

			var _messageDelivery = new MessageDelivery("test_publisher_confirms", typeof(MyEvent).Name, MessageDeliveryMode.Persistent, message => "ARoutingKey");
			var _myEvent = new MyEvent(Guid.NewGuid(), "CorrlationId_1", "Detail", 100);
			var _taskCompletionSource = new TaskCompletionSource<PublicationResult>();
			var _publication = new Publication(_messageDelivery, _myEvent, _taskCompletionSource);
			_publicationQueue.Add(_publication);

			_waitHandle.WaitOne();		// Allow channel publication to complete
			_channel.BasicNacks += Raise.EventWith(_channel, new BasicNackEventArgs { Multiple = true, DeliveryTag = 1UL });

			Assert.IsTrue(_publication.ResultTask.IsCompleted);
			Assert.AreEqual(PublicationResultStatus.Nacked, _publication.ResultTask.Result.Status);
			Assert.IsNull(_publication.ResultTask.Result.StatusContext);
		}


		[Test]
		public void Publish_Where_Two_Messages_Being_Published_But_Channel_Is_Closed_Before_Acks_Received_Results_In_Two_Channel_Shutdown_Tasks()
		{
			var _connection = Substitute.For<IConnection>();
			var _channel = Substitute.For<IModel>();
			var _publicationQueue = new BlockingCollection<Publication>();
			var _waitHandle = new CountdownEvent(2);

			_connection.CreateModel().Returns(_channel);
			_channel.NextPublishSeqNo.Returns(1Ul, 2UL);
			_channel
				.When(channel => channel.BasicPublish(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IBasicProperties>(), Arg.Any<byte[]>()))
				.Do(callInfo => _waitHandle.Signal());

			var _SUT = new PMCG.Messaging.Client.Publisher(_connection, _publicationQueue);
			_SUT.Start();

			var _messageDelivery = new MessageDelivery("test_publisher_confirms", typeof(MyEvent).Name, MessageDeliveryMode.Persistent, message => "ARoutingKey");
			var _myEvent = new MyEvent(Guid.NewGuid(), "CorrlationId_1", "Detail", 100);

			var _taskCompletionSource1 = new TaskCompletionSource<PublicationResult>();
			var _publication1 = new Publication(_messageDelivery, _myEvent, _taskCompletionSource1);
			var _taskCompletionSource2 = new TaskCompletionSource<PublicationResult>();
			var _publication2 = new Publication(_messageDelivery, _myEvent, _taskCompletionSource2);
			_publicationQueue.Add(_publication1);
			_publicationQueue.Add(_publication2);
			
			_waitHandle.Wait();		// Allow channel publications to complete
			_channel.ModelShutdown += Raise.EventWith(_channel, new ShutdownEventArgs(ShutdownInitiator.Peer, 1, "Bang!"));

			// Since all running on the same thread we do not need to wait - this is also not relaistic as we know the channel shutdown event will happen on a different thread
			Assert.IsTrue(_publication1.ResultTask.IsCompleted);
			Assert.AreEqual(PublicationResultStatus.ChannelShutdown, _publication1.ResultTask.Result.Status);
			Assert.IsTrue(_publication1.ResultTask.Result.StatusContext.Contains("Bang!"));
			Assert.IsTrue(_publication2.ResultTask.IsCompleted);
			Assert.AreEqual(PublicationResultStatus.ChannelShutdown, _publication2.ResultTask.Result.Status);
			Assert.IsTrue(_publication2.ResultTask.Result.StatusContext.Contains("Bang!"));
		}


		[Test]
		public void Publish_Where_Exchange_Does_Not_Exist_Results_In_Channel_Shutdown_And_A_Channel_Shutdown_Task_Result()
		{
			var _connection = Substitute.For<IConnection>();
			var _channel = Substitute.For<IModel>();
			var _publicationQueue = new BlockingCollection<Publication>();
			var _waitHandle = new AutoResetEvent(false);

			_connection.CreateModel().Returns(_channel);
			_channel.NextPublishSeqNo.Returns(1Ul);
			_channel
				.When(channel => channel.BasicPublish(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IBasicProperties>(), Arg.Any<byte[]>()))
				.Do(callInfo => _waitHandle.Set());

			var _SUT = new PMCG.Messaging.Client.Publisher(_connection, _publicationQueue);
			_SUT.Start();

			var _messageDelivery = new MessageDelivery("NON_EXISTENT_EXCHANGE", typeof(MyEvent).Name, MessageDeliveryMode.Persistent, message => "ARoutingKey");
			var _myEvent = new MyEvent(Guid.NewGuid(), "CorrlationId_1", "Detail", 100);
			var _taskCompletionSource = new TaskCompletionSource<PublicationResult>();
			var _publication = new Publication(_messageDelivery, _myEvent, _taskCompletionSource);
			_publicationQueue.Add(_publication);

			_waitHandle.WaitOne();		// Allow channel publication to complete
			_channel.ModelShutdown += Raise.EventWith(_channel, new ShutdownEventArgs(ShutdownInitiator.Peer, 1, "404 Exchange does not exist !"));

			// Since all running on the same thread we do not need to wait - this is also not relaistic as we know the channel shutdown event will happen on a different thread
			Assert.IsTrue(_publication.ResultTask.IsCompleted);
			Assert.AreEqual(PublicationResultStatus.ChannelShutdown, _publication.ResultTask.Result.Status);
			Assert.IsTrue(_publication.ResultTask.Result.StatusContext.Contains("404 Exchange does not exist !"));
		}
	}
}
