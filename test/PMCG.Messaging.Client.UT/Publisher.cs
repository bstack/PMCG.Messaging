using NSubstitute;
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
		private IConnection c_connection;
		private IModel c_channel;
		private BlockingCollection<Publication> c_publicationQueue;
		private MessageDelivery c_messageDelivery;
		private MyEvent c_myEvent;
		private TaskCompletionSource<PublicationResult> c_taskCompletionSource;


		[SetUp]
		public void SetUp()
		{
			this.c_connection = Substitute.For<IConnection>();
			this.c_channel = Substitute.For<IModel>();
			this.c_channel.IsOpen.Returns(true);
			this.c_connection.CreateModel().Returns(this.c_channel);
			this.c_publicationQueue = new BlockingCollection<Publication>();
			this.c_taskCompletionSource = new TaskCompletionSource<PublicationResult>();

			this.c_messageDelivery = new MessageDelivery("EXCHANGE", typeof(MyEvent).Name, MessageDeliveryMode.Persistent, message => "ARoutingKey");
			this.c_myEvent = new MyEvent(Guid.NewGuid(), "CorrlationId_1", "Detail", 100);
		}


		[Test]
		public void Publish_Failure_Unhandled_Exception_Task_Terminates()
		{
			this.c_channel.CreateBasicProperties().Returns(callInfo => { throw new Exception("Channel not open !"); });
			var _publication = new Publication(this.c_messageDelivery, this.c_myEvent, this.c_taskCompletionSource);
			var _SUT = new PMCG.Messaging.Client.Publisher(this.c_connection, this.c_publicationQueue);
			var _publisherTask = _SUT.Start();

			this.c_publicationQueue.Add(_publication);
			bool _isFaultedPublisherTask = false;
			try { _publisherTask.Wait(); } catch { _isFaultedPublisherTask = true; }

			Assert.IsTrue(_isFaultedPublisherTask);
		}


		[Test]
		public void Publish_Failure_Handled_Exception_Task_Continues()
		{
			this.c_channel
				.When(channel => channel.BasicPublish(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IBasicProperties>(), Arg.Any<byte[]>()))
				.Do(callInfo => { throw new ApplicationException("Bang !"); });
			var _publication = new Publication(this.c_messageDelivery, this.c_myEvent, this.c_taskCompletionSource);
			var _SUT = new PMCG.Messaging.Client.Publisher(this.c_connection, this.c_publicationQueue);
			var _publisherTask = _SUT.Start();

			this.c_publicationQueue.Add(_publication);
			// We wait 5 seconds to show that the task is still running and working - adding meesage back on to collection and trying to publish again in a continuous loop
			// Assert that the task has not completed as it is long lived.
			Task.Delay(5000).Wait();
			Thread.Sleep(TimeSpan.FromSeconds(5));

			Assert.IsFalse(_publication.ResultTask.IsCompleted);
		}


		[Test]
		public void Publish_Failure_Channel_Is_Closed_Task_Continues()
		{
			this.c_channel.IsOpen.Returns(false);
			var _publication = new Publication(this.c_messageDelivery, this.c_myEvent, this.c_taskCompletionSource);
			var _SUT = new PMCG.Messaging.Client.Publisher(this.c_connection, this.c_publicationQueue);
			var _publisherTask = _SUT.Start();

			this.c_publicationQueue.Add(_publication);

			// We wait 5 seconds to show that the task is still running and working - adding meesage back on to collection and trying to publish again in a continuous loop
			// Assert that the task has not completed as it is long lived.
			Thread.Sleep(TimeSpan.FromSeconds(5));
			Assert.IsFalse(_publication.ResultTask.IsCompleted);
		}


		[Test]
		public void Publish_Success_With_Ack()
		{
			var _waitHandle = new AutoResetEvent(false);
			var _messageProperties = Substitute.For<IBasicProperties>();
			this.c_channel.CreateBasicProperties().Returns(_messageProperties);
			this.c_channel.NextPublishSeqNo.Returns(1UL);
			this.c_channel
				.When(channel => channel.BasicPublish(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IBasicProperties>(), Arg.Any<byte[]>()))
				.Do(callInfo => _waitHandle.Set());
			var _SUT = new PMCG.Messaging.Client.Publisher(this.c_connection, this.c_publicationQueue);
			_SUT.Start(); 	// Can't capture result due to compiler treating warnings as errors - var is not used
			var _publication = new Publication(this.c_messageDelivery, this.c_myEvent, this.c_taskCompletionSource);

			this.c_publicationQueue.Add(_publication);
			_waitHandle.WaitOne();  // Allow publication to complete
			this.c_channel.BasicAcks += Raise.EventWith(this.c_channel, new BasicAckEventArgs { Multiple = false, DeliveryTag = 1 });
			
			Assert.IsTrue(_publication.ResultTask.IsCompleted);
			_messageProperties.Received().ContentType = "application/json";
			_messageProperties.Received().DeliveryMode = (byte)this.c_messageDelivery.DeliveryMode;
			_messageProperties.Received().Type = this.c_messageDelivery.TypeHeader;
			_messageProperties.Received().MessageId = this.c_myEvent.Id.ToString();
			_messageProperties.Received().CorrelationId = this.c_myEvent.CorrelationId;
			this.c_channel.Received().BasicPublish(this.c_messageDelivery.ExchangeName, this.c_messageDelivery.RoutingKeyFunc(this.c_myEvent), _messageProperties, Arg.Any<byte[]>());
		}


		[Test]
		public void Publish_Success_Multiple_Publications_All_Acked()
		{
			var _waitHandle = new CountdownEvent(10);
			var _nextPublishSeqNo = 1UL;
			this.c_channel.NextPublishSeqNo.Returns(callInfo => _nextPublishSeqNo++);       // Would not work when I used .Returns(1Ul, 2UL, 3UL); Not sure why this works !
			this.c_channel
				.When(channel => channel.BasicPublish(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IBasicProperties>(), Arg.Any<byte[]>()))
				.Do(callInfo => _waitHandle.Signal());
			var _SUT = new PMCG.Messaging.Client.Publisher(this.c_connection, this.c_publicationQueue);
			_SUT.Start();   // Can't capture due to compiler treating warnings as errors

			var _publications = new List<Publication>();
			for (var _index = 1; _index <= 10; _index++)
			{
				var _myEvent = new MyEvent(Guid.NewGuid(), "CorrlationId_1", "Detail", _index);
				var _taskCompletionSource = new TaskCompletionSource<PublicationResult>();
				var _publication = new Publication(this.c_messageDelivery, this.c_myEvent, _taskCompletionSource);

				this.c_publicationQueue.Add(_publication);
				_publications.Add(_publication);
			}
			_waitHandle.Wait();	// Allow publications to complete
			foreach(var _publication in _publications) { Assert.IsFalse(_publication.ResultTask.IsCompleted); }
			this.c_channel.BasicAcks += Raise.EventWith(this.c_channel, new BasicAckEventArgs { Multiple = true, DeliveryTag = (ulong)_publications.Count });

			foreach (var _publication in _publications)
			{
				Assert.IsTrue(_publication.ResultTask.IsCompleted);
				Assert.AreEqual(PublicationResultStatus.Acked, _publication.ResultTask.Result.Status);
			}
		}


		[Test]
		public void Publish_Success_Multiple_Publications_Only_Some_Acked()
		{
			var _waitHandle = new CountdownEvent(100);
			var _nextPublishSeqNo = 1UL;
			this.c_channel.NextPublishSeqNo.Returns(callInfo => _nextPublishSeqNo++);       // Would not work when I used .Returns(1Ul, 2UL, 3UL); Not sure why this works !
			this.c_channel
				.When(channel => channel.BasicPublish(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IBasicProperties>(), Arg.Any<byte[]>()))
				.Do(callInfo => _waitHandle.Signal());
			var _SUT = new PMCG.Messaging.Client.Publisher(this.c_connection, this.c_publicationQueue);
			_SUT.Start();

			var _publications = new List<Publication>();
			for (var _index = 1; _index <= 100; _index++)
			{
				var _myEvent = new MyEvent(Guid.NewGuid(), "CorrlationId_1", "Detail", _index);
				var _taskCompletionSource = new TaskCompletionSource<PublicationResult>();
				var _publication = new Publication(this.c_messageDelivery, _myEvent, _taskCompletionSource);

				this.c_publicationQueue.Add(_publication);
				_publications.Add(_publication);
			}
			_waitHandle.Wait();		// Allow channel publications to complete
			foreach(var _publication in _publications) { Assert.IsFalse(_publication.ResultTask.IsCompleted); }
			var _deliveryTagToAcknowledge = 73;
			this.c_channel.BasicAcks += Raise.EventWith(this.c_channel, new BasicAckEventArgs { Multiple = true, DeliveryTag = (ulong)_deliveryTagToAcknowledge });

			Assert.AreEqual(_deliveryTagToAcknowledge, _publications.Count(publication => publication.ResultTask.IsCompleted), "A1");
			Assert.AreEqual(_deliveryTagToAcknowledge, _publications.Count(publication => publication.ResultTask.IsCompleted && publication.ResultTask.Result.Status == PublicationResultStatus.Acked), "A2");
			Assert.AreEqual(100 - _deliveryTagToAcknowledge, _publications.Count(publication => !publication.ResultTask.IsCompleted), "A3");
		}


		[Test]
		public void Publish_Success_Nacked()
		{
			var _waitHandle = new AutoResetEvent(false);
			this.c_channel.NextPublishSeqNo.Returns(1Ul);
			this.c_channel
				.When(channel => channel.BasicPublish(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IBasicProperties>(), Arg.Any<byte[]>()))
				.Do(callInfo => _waitHandle.Set());
			var _SUT = new PMCG.Messaging.Client.Publisher(this.c_connection, this.c_publicationQueue);
			_SUT.Start();
			
			var _publication = new Publication(this.c_messageDelivery, this.c_myEvent, this.c_taskCompletionSource);
			this.c_publicationQueue.Add(_publication);
			_waitHandle.WaitOne();      // Allow channel publication to complete
			this.c_channel.BasicNacks += Raise.EventWith(this.c_channel, new BasicNackEventArgs { Multiple = true, DeliveryTag = 1UL });

			Assert.IsTrue(_publication.ResultTask.IsCompleted);
			Assert.AreEqual(PublicationResultStatus.Nacked, _publication.ResultTask.Result.Status);
			Assert.IsNull(_publication.ResultTask.Result.StatusContext);
		}


		[Test]
		public void Publish_Success_Channel_Shutdown_Before_Acked()
		{
			var _waitHandle = new CountdownEvent(2);
			this.c_channel.NextPublishSeqNo.Returns(1Ul, 2UL);
			this.c_channel
				.When(channel => channel.BasicPublish(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IBasicProperties>(), Arg.Any<byte[]>()))
				.Do(callInfo => _waitHandle.Signal());
			var _SUT = new PMCG.Messaging.Client.Publisher(this.c_connection, this.c_publicationQueue);
			_SUT.Start();
			var _taskCompletionSource1 = new TaskCompletionSource<PublicationResult>();
			var _publication1 = new Publication(this.c_messageDelivery, this.c_myEvent, _taskCompletionSource1);
			var _taskCompletionSource2 = new TaskCompletionSource<PublicationResult>();
			var _publication2 = new Publication(this.c_messageDelivery, this.c_myEvent, _taskCompletionSource2);

			this.c_publicationQueue.Add(_publication1);
			this.c_publicationQueue.Add(_publication2);
			_waitHandle.Wait();     // Allow channel publications to complete
			this.c_channel.ModelShutdown += Raise.EventWith(this.c_channel, new ShutdownEventArgs(ShutdownInitiator.Peer, 1, "Bang!"));

			// Since all running on the same thread we do not need to wait - this is also not realistic as we know the channel shutdown event will happen on a different thread
			Assert.IsTrue(_publication1.ResultTask.IsCompleted);
			Assert.AreEqual(PublicationResultStatus.ChannelShutdown, _publication1.ResultTask.Result.Status);
			Assert.IsTrue(_publication1.ResultTask.Result.StatusContext.Contains("Bang!"));
			Assert.IsTrue(_publication2.ResultTask.IsCompleted);
			Assert.AreEqual(PublicationResultStatus.ChannelShutdown, _publication2.ResultTask.Result.Status);
			Assert.IsTrue(_publication2.ResultTask.Result.StatusContext.Contains("Bang!"));
		}


		[Test]
		public void Publish_Success_Channel_Closed_And_Then_Opened()
		{
			this.c_channel.IsOpen.Returns(false);
			var _publicationQueue = new BlockingCollection<Publication>();
			var _messageProperties = Substitute.For<IBasicProperties>();
			var _waitHandle = new AutoResetEvent(false);
			this.c_channel.CreateBasicProperties().Returns(_messageProperties);
			this.c_channel.NextPublishSeqNo.Returns(1UL);
			this.c_channel
				.When(channel => channel.BasicPublish(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IBasicProperties>(), Arg.Any<byte[]>()))
				.Do(callInfo => _waitHandle.Set());
			var _SUT = new PMCG.Messaging.Client.Publisher(this.c_connection, _publicationQueue);
			_SUT.Start();   // Can't capture result due to compiler treating warnings as errors - var is not used

			var _publication = new Publication(this.c_messageDelivery, this.c_myEvent, this.c_taskCompletionSource);
			_publicationQueue.Add(_publication);
			Task.Delay(2000).Wait();
			this.c_channel.IsOpen.Returns(true); // Simulate channel recovering
			_waitHandle.WaitOne();  // Allow publication to complete
			this.c_channel.BasicAcks += Raise.EventWith(this.c_channel, new BasicAckEventArgs { Multiple = false, DeliveryTag = 1 });

			Assert.IsTrue(_publication.ResultTask.IsCompleted);
			_messageProperties.Received().ContentType = "application/json";
			_messageProperties.Received().DeliveryMode = (byte)this.c_messageDelivery.DeliveryMode;
			_messageProperties.Received().Type = this.c_messageDelivery.TypeHeader;
			_messageProperties.Received().MessageId = this.c_myEvent.Id.ToString();
			_messageProperties.Received().CorrelationId = this.c_myEvent.CorrelationId;
			this.c_channel.Received().BasicPublish(this.c_messageDelivery.ExchangeName, this.c_messageDelivery.RoutingKeyFunc(this.c_myEvent), _messageProperties, Arg.Any<byte[]>());
		}
	}
}