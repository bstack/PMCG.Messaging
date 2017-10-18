using PMCG.Messaging.Client.Configuration;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace PMCG.Messaging.Client.AT.Publish
{
	public class Tests
	{
		public void Publish_A_Message_To_A_Queue_Using_The_Direct_Exchange()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder(Accessories.Configuration.ConnectionSettingsString);
			_busConfigurationBuilder
				.RegisterPublication<Accessories.MyEvent>("", typeof(Accessories.MyEvent).Name, MessageDeliveryMode.Persistent, message => Accessories.Configuration.QueueName1);
			var _SUT = new Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			var _message = new Accessories.MyEvent(Guid.NewGuid(), "", "R1", 1, "09:00", "DDD....");
			var _result = _SUT.PublishAsync(_message);
			_result.Wait(TimeSpan.FromSeconds(1));

			Console.WriteLine(string.Format("TaskStatus expected: (RanToCompletion), actual: ({0})", _result.Status));
			Console.WriteLine(string.Format("PublicationResultStatus expected: (Published), actual: ({0})", _result.Result.Status));
			Console.WriteLine("Verify a message was sent to AMPQ default exchange in management ui");
			Console.Read();
		}


		public void Publish_A_Message_To_A_Queue_Using_Custom_Exchange()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder(Accessories.Configuration.ConnectionSettingsString);
			_busConfigurationBuilder
				.RegisterPublication<Accessories.MyEvent>(Accessories.Configuration.ExchangeName1, typeof(Accessories.MyEvent).Name, MessageDeliveryMode.Persistent, message => Accessories.Configuration.QueueName1);
			var _SUT = new Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			var _message = new Accessories.MyEvent(Guid.NewGuid(), "", "R1", 1, "09:00", "DDD....");
			var _result = _SUT.PublishAsync(_message);
			_result.Wait(TimeSpan.FromSeconds(1));

			Console.WriteLine(string.Format("TaskStatus expected: (RanToCompletion), actual: ({0})", _result.Status));
			Console.WriteLine(string.Format("PublicationResultStatus expected: (Published), actual: ({0})", _result.Result.Status));
			Console.WriteLine("Verify a message is on the queue in management ui");
			Console.Read();
		}


		public void Publish_A_Message_To_Two_Exchanges()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder(Accessories.Configuration.ConnectionSettingsString);
			_busConfigurationBuilder
				.RegisterPublication<Accessories.MyEvent>(Accessories.Configuration.ExchangeName1, typeof(Accessories.MyEvent).Name, MessageDeliveryMode.Persistent, message => Accessories.Configuration.QueueName1)
				.RegisterPublication<Accessories.MyEvent>(Accessories.Configuration.ExchangeName2, typeof(Accessories.MyEvent).Name, MessageDeliveryMode.Persistent, message => Accessories.Configuration.QueueName2);
			var _SUT = new Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			var _message = new Accessories.MyEvent(Guid.NewGuid(), "", "R1", 1, "09:00", "DDD....");
			var _result = _SUT.PublishAsync(_message);
			_result.Wait(TimeSpan.FromSeconds(1));

			Console.WriteLine(string.Format("TaskStatus expected: (RanToCompletion), actual: ({0})", _result.Status));
			Console.WriteLine(string.Format("PublicationResultStatus expected: (Published), actual: ({0})", _result.Result.Status));
			Console.WriteLine("Verify that queue1 and queue 2 receive 1 message each on management ui");
			Console.Read();
		}



		public void Publish_A_Message_To_An_Exchange_That_Doesnt_Exist()
		{
			// The channel shutdown event is invoked by RabbitMQ server as the exchange does not exist.
			// This is an unrecoverable scenario that we cannot cater for, however it should never happen anyway as there should never be an incorrectly configured exchange

			var _busConfigurationBuilder = new BusConfigurationBuilder(Accessories.Configuration.ConnectionSettingsString);
			_busConfigurationBuilder
				.RegisterPublication<Accessories.MyEvent>("exchange_that_doesnt_exist", typeof(Accessories.MyEvent).Name, MessageDeliveryMode.Persistent, message => Accessories.Configuration.QueueName1);
			var _SUT = new Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			var _message = new Accessories.MyEvent(Guid.NewGuid(), "", "R1", 1, "09:00", "DDD....");
			var _result = _SUT.PublishAsync(_message);
			_result.Wait(TimeSpan.FromSeconds(1));

			Console.WriteLine(string.Format("TaskStatus expected: (RanToCompletion), actual: ({0})", _result.Status));
			Console.WriteLine(string.Format("PublicationResultStatus expected: (NotPublished), actual: ({0})", _result.Result.Status));
			Console.Read();
		}


		public void Publish_Nack_Returned_From_RabbitMQ_Server()
		{
			// We have this tested in UTs but it is impossible to test at AT level here. 
			// This is because basic.nack will only be delivered if an internal error occurs in the Erlang process responsible for a queue. 
			// See https://www.rabbitmq.com/confirms.html for more info
			// However we have this represented as a test placeholder inside here so we dont forget why we didnt have a test for this scenario
		}


		public void Publish_1000_Messages_To_A_Queue_Using_Custom_Exchange()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder(Accessories.Configuration.ConnectionSettingsString);
			_busConfigurationBuilder
				.RegisterPublication<Accessories.MyEvent>(Accessories.Configuration.ExchangeName1, typeof(Accessories.MyEvent).Name, MessageDeliveryMode.Persistent, message => Accessories.Configuration.QueueName1);
			var _SUT = new Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			var _tasks = new ConcurrentBag<Task<PMCG.Messaging.PublicationResult>>();
			for (int count = 0; count < 1000; count++)
			{
				var _message = new Accessories.MyEvent(Guid.NewGuid(), "", "R1", 1, "09:00", "DDD....");
				_tasks.Add(_SUT.PublishAsync(_message));
			}

			Task.WhenAll(_tasks).ContinueWith(a =>
			{
				var _taskStatusCount = _tasks.Count(result => result.Status == TaskStatus.RanToCompletion);
				var _messagePublishedCount = _tasks.Count(result => result.Result.Status == Messaging.PublicationResultStatus.Published);
				Console.WriteLine(string.Format("RanToCompletionTaskCount expected: (1000), actual: ({0})", _taskStatusCount));
				Console.WriteLine(string.Format("MessagePublishedCount expected: (1000), actual: ({0})", _messagePublishedCount));
				Console.WriteLine("Verify that 1000 messages on queue on management ui");
			});
			
			Console.Read();
		}

		// TODO: What is this test for???
		public void Publish_With_Timeout()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder(Accessories.Configuration.ConnectionSettingsString);
			_busConfigurationBuilder
				.RegisterPublication<Accessories.MyEvent>(Accessories.Configuration.ExchangeName1, typeof(Accessories.MyEvent).Name, MessageDeliveryMode.Persistent, message => Accessories.Configuration.QueueName1);
			var _SUT = new Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			var _message = new Accessories.MyEvent(Guid.NewGuid(), "", "R1", 1, "09:00", "DDD....");
			var _result = _SUT.PublishAsync(_message);

			if (!_result.Wait(TimeSpan.FromMilliseconds(1)))
			{
				Console.WriteLine(string.Format("TaskStatus expected: (WaitingForActivation), actual: ({0})", _result.Status));
			}

			Console.WriteLine("Verify that 1 message on queue on management ui");
			Console.Read();
		}


		public void Publish_Connection_Closed_By_Application_Never_Recovers()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder(Accessories.Configuration.ConnectionSettingsString);
			_busConfigurationBuilder
				.RegisterPublication<Accessories.MyEvent>(Accessories.Configuration.ExchangeName1, typeof(Accessories.MyEvent).Name, MessageDeliveryMode.Persistent, message => Accessories.Configuration.QueueName1);
			var _SUT = new Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();
			_SUT.Close();

			var _message = new Accessories.MyEvent(Guid.NewGuid(), "", "R1", 1, "09:00", "DDD....");
			var _result = _SUT.PublishAsync(_message);
			
			Console.WriteLine("Should keep retrying to publish every second indefinitely");
			Console.WriteLine("This is due to the fact that the application initiated the close");
			Console.WriteLine("Normally, close is only called when application is closing down");
			Console.Read();
		}


		public void Publish_Connection_Closed_By_Server_Recovers_Automatically()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder(Accessories.Configuration.ConnectionSettingsString);
			_busConfigurationBuilder
				.RegisterPublication<Accessories.MyEvent>(Accessories.Configuration.ExchangeName1, typeof(Accessories.MyEvent).Name, MessageDeliveryMode.Persistent, message => Accessories.Configuration.QueueName1);
			var _SUT = new Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			// This test manually takes the following steps:
			//		1 - We publish continuously for 40,000 messages
			//		2 - We close the connection via the UI by clicking on connections, forcing close
			//		3 - Observe that for a period of time it fails to publish , retrying every second (via logs)
			//		4 - Observe that the connection is automatically recovered after a period of time (via management ui)
			//		5 - Observe that all failed messages have since published successfully (via logs)

			var _tasks = new ConcurrentBag<Task<PMCG.Messaging.PublicationResult>>();
			for (int count = 0; count < 40000; count++)
			{
				var _message = new Accessories.MyEvent(Guid.NewGuid(), "", "R1", 1, "09:00", "DDD....");
				_tasks.Add(_SUT.PublishAsync(_message));
				Console.WriteLine(count);
			}

			Task.WhenAll(_tasks).ContinueWith(a =>
			{
				var _taskStatusCount = _tasks.Count(result => result.Status == TaskStatus.RanToCompletion);
				var _messagePublishedCount = _tasks.Count(result => result.Result.Status == Messaging.PublicationResultStatus.Published);
				Console.WriteLine(string.Format("RanToCompletionTaskCount expected: (40000), actual: ({0})", _taskStatusCount));
				Console.WriteLine(string.Format("MessagePublishedCount expected: (40000), actual: ({0})", _messagePublishedCount));
			});

			Console.Read();
		}


		public void Publish_Connection_Closed_By_Server_Restart_Unpublished_Messages_Are_Republished_Successfully()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder(Accessories.Configuration.ConnectionSettingsString);
			_busConfigurationBuilder
				.RegisterPublication<Accessories.MyEvent>(Accessories.Configuration.ExchangeName1, typeof(Accessories.MyEvent).Name, MessageDeliveryMode.Persistent, message => Accessories.Configuration.QueueName1);
			var _SUT = new Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			// This test manually takes the following steps:
			//		1 - We publish every second for 1 minute
			//		2 - We restart the rabbitmq server
			//		3 - Observe that for a period of time it fails to publish , retrying every second (via logs)
			//		4 - Observe that the connection is automatically recovered after a period of time (via management ui)
			//		5 - Observe that all failed messages have since published successfully (via logs)

			var _tasks = new ConcurrentBag<Task<PMCG.Messaging.PublicationResult>>();
			for (int count = 0; count < 60; count++)
			{
				Thread.Sleep(1000);
				var _message = new Accessories.MyEvent(Guid.NewGuid(), "", "R1", 1, "09:00", "DDD....");
				_tasks.Add(_SUT.PublishAsync(_message));
			}

			Task.WhenAll(_tasks).ContinueWith(a =>
			{
				var _taskStatusCount = _tasks.Count(result => result.Status == TaskStatus.RanToCompletion);
				var _messagePublishedCount = _tasks.Count(result => result.Result.Status == Messaging.PublicationResultStatus.Published);
				Console.WriteLine(string.Format("RanToCompletionTaskCount expected: (60), actual: ({0})", _taskStatusCount));
				Console.WriteLine(string.Format("MessagePublishedCount expected: (60), actual: ({0})", _messagePublishedCount));
				Console.WriteLine("Verify that 60 message on queue on management ui");
			});

			Console.Read();
		}


		public void Publish_Connection_Blocked_Then_Unblocked_Unpublished_Messages_Are_Republished_Successfully()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder(Accessories.Configuration.ConnectionSettingsString);
			_busConfigurationBuilder
				.RegisterPublication<Accessories.MyEvent>(Accessories.Configuration.ExchangeName1, typeof(Accessories.MyEvent).Name, MessageDeliveryMode.Persistent, message => Accessories.Configuration.QueueName1);
			var _SUT = new Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			// This test manually takes the following steps:
			//		1 - We publish every second for 1 minute
			//		2 - We block connection by setting memory high watermark to a very low setting 
			//			./rabbitmqctl.bat set_vm_memory_high_watermark 0.0000001
			//		3 - Observe that for a period of time it fails to publish , retrying every second (via logs)
			//		4 - We unblock connection by setting memory high watermark back to normal setting
			//			./rabbitmqctl.bat set_vm_memory_high_watermark 0.4
			//		5 - Observe that the connection is automatically recovered after a period of time (via UI)
			//		6 - Observe that all failed messages have since published successfully (via logs)

			var _tasks = new ConcurrentBag<Task<PMCG.Messaging.PublicationResult>>();
			for (int count = 0; count < 60; count++)
			{
				Thread.Sleep(1000);
				var _message = new Accessories.MyEvent(Guid.NewGuid(), "", "R1", 1, "09:00", "DDD....");
				_tasks.Add(_SUT.PublishAsync(_message));
			}

			Task.WhenAll(_tasks).ContinueWith(a =>
			{
				var _taskStatusCount = _tasks.Count(result => result.Status == TaskStatus.RanToCompletion);
				var _messagePublishedCount = _tasks.Count(result => result.Result.Status == Messaging.PublicationResultStatus.Published);
				Console.WriteLine(string.Format("RanToCompletionTaskCount expected: (60), actual: ({0})", _taskStatusCount));
				Console.WriteLine(string.Format("MessagePublishedCount expected: (60), actual: ({0})", _messagePublishedCount));
				Console.WriteLine("Verify that 60 message on queue on management ui");
			});

			Console.Read();
		}


		public void Publish_Invalid_Message_Is_Null()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder(Accessories.Configuration.ConnectionSettingsString);
			_busConfigurationBuilder
				.RegisterPublication<Accessories.MyEvent>(Accessories.Configuration.ExchangeName1, typeof(Accessories.MyEvent).Name, MessageDeliveryMode.Persistent, message => Accessories.Configuration.QueueName1);
			var _SUT = new Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			// This is an unrecoverable scenario that we cannot cater for, however it should never happen anyway as there should never be a null message
			try
			{
				Accessories.MyEvent _message = null;
				var _result = _SUT.PublishAsync(_message);
				_result.Wait(TimeSpan.FromSeconds(1));
			}
			catch (Exception exception)
			{
				Console.WriteLine(string.Format("Exception expected: (Value cannot be null.Parameter name: message), actual: ({0})", exception.Message));
			}

			Console.Read();
		}


		public void Publish_A_Message_That_Expires_Ends_Up_In_Dead_Letter_Queue()
		{
			var _capturedMessageId = string.Empty;

			var _busConfigurationBuilder = new BusConfigurationBuilder(Accessories.Configuration.ConnectionSettingsString);
			_busConfigurationBuilder.NumberOfConsumers = 2;
			_busConfigurationBuilder
				.RegisterPublication<Accessories.MyEvent>(Accessories.Configuration.ExchangeName2, "test.queue.2", MessageDeliveryMode.Persistent, message => "test.queue.2");
			
			var _SUT = new Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			var _messageId = Guid.NewGuid();
			var _message = new Accessories.MyEvent(_messageId, null, "R1", 1, "09:00", "DDD....");
			_SUT.PublishAsync(_message);

			Console.WriteLine(string.Format("Ensure message with id: ({0}) no longer exists in the queue", _messageId));
			Console.WriteLine(string.Format("Ensure message with id: ({0}) exists in the dead letter queue - expired", _messageId));
			Console.Read();
		}
	}
}
