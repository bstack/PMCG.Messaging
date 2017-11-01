using PMCG.Messaging.Client.Configuration;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace PMCG.Messaging.Client.AT.Cluster
{
	public class Tests
	{
		public void Reboot_Node_In_The_Cluster_Where_Queues_Exists()
		{
			// Let node that queues were created on = NodeA
			// Let other node = NodeB

			// Before reboot of NodeA, verify cluster is in working order
			// Reboot NodeA
			// During reboot, verify that cluster still exists conceptually, but one NodeA is down (Node not running)
			// After successful reboot, verify that cluster is restored back to working order
			// Verify that all queues now exist on other NodeB
		}


		public void Reboot_Node_In_The_Cluster_Where_Queues_Does_Not_Exist()
		{
			// Let node that queues were created on = NodeA
			// Let other node = NodeB

			// Before reboot of NodeB, verify cluster is in working order
			// Reboot NodeB
			// During reboot, verify that cluster still exists conceptually, but one NodeB is down (Node not running)
			// After successful reboot, verify that cluster is restored back to working order
			// Verify that all queues still exist on NodeA
		}


		public void Publish_Continues_When_Node_We_Are_Not_Connected_To_Has_RabbitMQ_Service_Stopped()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder(Accessories.Configuration.ConnectionSettingsString);
			_busConfigurationBuilder
				.RegisterPublication<Accessories.MyEvent>(Accessories.Configuration.ExchangeName1, typeof(Accessories.MyEvent).Name, MessageDeliveryMode.Persistent, message => Accessories.Configuration.QueueName1);
			var _SUT = new Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			// Let node that we are connected to = NodeA
			// Let other node = NodeB

			// Before stopping rabbitmq service on NodeB, verify cluster is in working order
			// We publish continuously for 10,000 messages
			// During publication, stop rabbitmq service on NodeB
			//		service rabbitmq-server stop
			// Verify 10,000 messages exist on queue
			// Start rabbitmq service on NodeB

			var _tasks = new ConcurrentBag<Task<PMCG.Messaging.PublicationResult>>();
			for (int count = 0; count < 10000; count++)
			{
				var _message = new Accessories.MyEvent(Guid.NewGuid(), "", "R1", 1, "09:00", "DDD....");
				_tasks.Add(_SUT.PublishAsync(_message));
				Console.WriteLine(count);
				Thread.Sleep(1);
			}

			Task.WhenAll(_tasks).ContinueWith(a =>
			{
				var _taskStatusCount = _tasks.Count(result => result.Status == TaskStatus.RanToCompletion);
				var _messagePublishedCount = _tasks.Count(result => result.Result.Status == Messaging.PublicationResultStatus.Published);
				Console.WriteLine(string.Format("RanToCompletionTaskCount expected: (10000), actual: ({0})", _taskStatusCount));
				Console.WriteLine(string.Format("MessagePublishedCount expected: (10000), actual: ({0})", _messagePublishedCount));
			});

			Console.Read();
		}


		public void Publish_Continues_When_Node_We_Are_Connected_To_Has_RabbitMQ_Service_Stopped()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder(Accessories.Configuration.ConnectionSettingsString);
			_busConfigurationBuilder
				.RegisterPublication<Accessories.MyEvent>(Accessories.Configuration.ExchangeName1, typeof(Accessories.MyEvent).Name, MessageDeliveryMode.Persistent, message => Accessories.Configuration.QueueName1);
			var _SUT = new Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			// Let node that we are connected to = NodeA
			// Let other node = NodeB

			// Before stopping rabbitmq service on NodeA, verify cluster is in working order
			// We publish continuously for 10,000 messages
			// During publication, stop rabbitmq service on NodeA
			//		service rabbitmq-server stop
			// Verify 10,000 messages exist on queue
			// Verify we are now connected to NodeB (via automatic recovery)
			// Start rabbitmq service on NodeA

			var _tasks = new ConcurrentBag<Task<PMCG.Messaging.PublicationResult>>();
			for (int count = 0; count < 10000; count++)
			{
				var _message = new Accessories.MyEvent(Guid.NewGuid(), "", "R1", 1, "09:00", "DDD....");
				_tasks.Add(_SUT.PublishAsync(_message));
				Console.WriteLine(count);
				Thread.Sleep(1);
			}

			Task.WhenAll(_tasks).ContinueWith(a =>
			{
				var _taskStatusCount = _tasks.Count(result => result.Status == TaskStatus.RanToCompletion);
				var _messagePublishedCount = _tasks.Count(result => result.Result.Status == Messaging.PublicationResultStatus.Published);
				Console.WriteLine(string.Format("RanToCompletionTaskCount expected: (10000), actual: ({0})", _taskStatusCount));
				Console.WriteLine(string.Format("MessagePublishedCount expected: (10000), actual: ({0})", _messagePublishedCount));
			});

			Console.Read();
		}


		public void Publish_Continues_When_Node_We_Are_Not_Connected_Is_Rebooted()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder(Accessories.Configuration.ConnectionSettingsString);
			_busConfigurationBuilder
				.RegisterPublication<Accessories.MyEvent>(Accessories.Configuration.ExchangeName1, typeof(Accessories.MyEvent).Name, MessageDeliveryMode.Persistent, message => Accessories.Configuration.QueueName1);
			var _SUT = new Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			// Let node that we are connected to = NodeA
			// Let other node = NodeB

			// Before rebooting NodeB, verify cluster is in working order
			// We publish continuously for 10,000 messages
			// During publication, reboot NodeB
			//		reboot
			// Verify 10,000 messages exist on queue
			// Verify cluster is in working order after reboot

			var _tasks = new ConcurrentBag<Task<PMCG.Messaging.PublicationResult>>();
			for (int count = 0; count < 10000; count++)
			{
				var _message = new Accessories.MyEvent(Guid.NewGuid(), "", "R1", 1, "09:00", "DDD....");
				_tasks.Add(_SUT.PublishAsync(_message));
				Console.WriteLine(count);
				Thread.Sleep(1);
			}

			Task.WhenAll(_tasks).ContinueWith(a =>
			{
				var _taskStatusCount = _tasks.Count(result => result.Status == TaskStatus.RanToCompletion);
				var _messagePublishedCount = _tasks.Count(result => result.Result.Status == Messaging.PublicationResultStatus.Published);
				Console.WriteLine(string.Format("RanToCompletionTaskCount expected: (10000), actual: ({0})", _taskStatusCount));
				Console.WriteLine(string.Format("MessagePublishedCount expected: (10000), actual: ({0})", _messagePublishedCount));
			});

			Console.Read();
		}


		public void Publish_Continues_When_Node_We_Are_Connected_To_Is_Rebooted()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder(Accessories.Configuration.ConnectionSettingsString);
			_busConfigurationBuilder
				.RegisterPublication<Accessories.MyEvent>(Accessories.Configuration.ExchangeName1, typeof(Accessories.MyEvent).Name, MessageDeliveryMode.Persistent, message => Accessories.Configuration.QueueName1);
			var _SUT = new Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			// Let node that we are connected to = NodeA
			// Let other node = NodeB

			// Before rebooting NodeA, verify cluster is in working order
			// We publish continuously for 10,000 messages
			// During publication, reboot NodeA
			//		reboot
			// Verify 10,000 messages exist on queue
			// Verify we are now connected to NodeB (via automatic recovery)
			// Verify cluster is in working order after reboot

			var _tasks = new ConcurrentBag<Task<PMCG.Messaging.PublicationResult>>();
			for (int count = 0; count < 10000; count++)
			{
				var _message = new Accessories.MyEvent(Guid.NewGuid(), "", "R1", 1, "09:00", "DDD....");
				_tasks.Add(_SUT.PublishAsync(_message));
				Console.WriteLine(count);
				Thread.Sleep(1);
			}

			Task.WhenAll(_tasks).ContinueWith(a =>
			{
				var _taskStatusCount = _tasks.Count(result => result.Status == TaskStatus.RanToCompletion);
				var _messagePublishedCount = _tasks.Count(result => result.Result.Status == Messaging.PublicationResultStatus.Published);
				Console.WriteLine(string.Format("RanToCompletionTaskCount expected: (10000), actual: ({0})", _taskStatusCount));
				Console.WriteLine(string.Format("MessagePublishedCount expected: (10000), actual: ({0})", _messagePublishedCount));
			});

			Console.Read();
		}


		public void Publish_Continues_When_Both_Nodes_Are_Rebooted_Consecutively_Simulating_Ubuntu_Updates()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder(Accessories.Configuration.ConnectionSettingsString);
			_busConfigurationBuilder
				.RegisterPublication<Accessories.MyEvent>(Accessories.Configuration.ExchangeName1, typeof(Accessories.MyEvent).Name, MessageDeliveryMode.Persistent, message => Accessories.Configuration.QueueName1);
			var _SUT = new Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			// Let node that we are connected to = NodeA
			// Let other node = NodeB

			// Before rebooting NodeA, verify cluster is in working order
			// We publish continuously for 15,000 messages
			// During publication, reboot NodeA
			//		reboot
			// Wait for the reboot to finish, verify cluster is in working order
			// Synchronize the queue (if required, otherwise this may result in lost messages)
			// During publication, reboot NodeB
			//		reboot
			// Wait for the reboot to finish, verify cluster is in working order
			// Verify 15,000 messages exist on queue

			var _tasks = new ConcurrentBag<Task<PMCG.Messaging.PublicationResult>>();
			for (int count = 0; count < 15000; count++)
			{
				var _message = new Accessories.MyEvent(Guid.NewGuid(), "", "R1", 1, "09:00", "DDD....");
				_tasks.Add(_SUT.PublishAsync(_message));
				Console.WriteLine(count);
				Thread.Sleep(10);
			}

			Task.WhenAll(_tasks).ContinueWith(a =>
			{
				var _taskStatusCount = _tasks.Count(result => result.Status == TaskStatus.RanToCompletion);
				var _messagePublishedCount = _tasks.Count(result => result.Result.Status == Messaging.PublicationResultStatus.Published);
				Console.WriteLine(string.Format("RanToCompletionTaskCount expected: (10000), actual: ({0})", _taskStatusCount));
				Console.WriteLine(string.Format("MessagePublishedCount expected: (10000), actual: ({0})", _messagePublishedCount));
			});

			Console.Read();
		}


		public void Publish_Continues_When_Node_We_Are_Not_Connected_Is_Blocked()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder(Accessories.Configuration.ConnectionSettingsString);
			_busConfigurationBuilder
				.RegisterPublication<Accessories.MyEvent>(Accessories.Configuration.ExchangeName1, typeof(Accessories.MyEvent).Name, MessageDeliveryMode.Persistent, message => Accessories.Configuration.QueueName1);
			var _SUT = new Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			// Let node that we are connected to = NodeA
			// Let other node = NodeB

			// Before blocking NodeB, verify cluster is in working order
			// We publish continuously for 10,000 messages
			// During publication, block NodeB for a period of time, then unblock
			//		rabbitmqctl set_vm_memory_high_watermark 0.0000001
			//		rabbitmqctl set_vm_memory_high_watermark 0.4
			// Verify 10,000 messages exist on queue
			// Verify cluster is in working order

			var _tasks = new ConcurrentBag<Task<PMCG.Messaging.PublicationResult>>();
			for (int count = 0; count < 10000; count++)
			{
				var _message = new Accessories.MyEvent(Guid.NewGuid(), "", "R1", 1, "09:00", "DDD....");
				_tasks.Add(_SUT.PublishAsync(_message));
				Console.WriteLine(count);
				Thread.Sleep(1);
			}

			Task.WhenAll(_tasks).ContinueWith(a =>
			{
				var _taskStatusCount = _tasks.Count(result => result.Status == TaskStatus.RanToCompletion);
				var _messagePublishedCount = _tasks.Count(result => result.Result.Status == Messaging.PublicationResultStatus.Published);
				Console.WriteLine(string.Format("RanToCompletionTaskCount expected: (10000), actual: ({0})", _taskStatusCount));
				Console.WriteLine(string.Format("MessagePublishedCount expected: (10000), actual: ({0})", _messagePublishedCount));
			});

			Console.Read();
		}


		public void Publish_Continues_When_Node_We_Are_Connected_To_Is_Blocked()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder(Accessories.Configuration.ConnectionSettingsString);
			_busConfigurationBuilder
				.RegisterPublication<Accessories.MyEvent>(Accessories.Configuration.ExchangeName1, typeof(Accessories.MyEvent).Name, MessageDeliveryMode.Persistent, message => Accessories.Configuration.QueueName1);
			var _SUT = new Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			// Let node that we are connected to = NodeA
			// Let other node = NodeB

			// Before blocking NodeA, verify cluster is in working order
			// We publish continuously for 10,000 messages
			// During publication, block NodeA for a period of time, then unblock
			//		rabbitmqctl set_vm_memory_high_watermark 0.0000001
			//		rabbitmqctl set_vm_memory_high_watermark 0.4
			// Verify 10,000 messages exist on queue
			// Verify cluster is in working order

			// NOTE: When we run high volume very fast we see messages go missing, we do not think this is worth chasing up on
			// We also know that for some edge cases, channel.BasicPublish can execute where the messages simply go missing, this is simply another one of these use cases

			var _tasks = new ConcurrentBag<Task<PMCG.Messaging.PublicationResult>>();
			for (int count = 0; count < 10000; count++)
			{
				var _message = new Accessories.MyEvent(Guid.NewGuid(), "", "R1", 1, "09:00", "DDD....");
				_tasks.Add(_SUT.PublishAsync(_message));
				Console.WriteLine(count);
				Thread.Sleep(1);
			}

			Task.WhenAll(_tasks).ContinueWith(a =>
			{
				var _taskStatusCount = _tasks.Count(result => result.Status == TaskStatus.RanToCompletion);
				var _messagePublishedCount = _tasks.Count(result => result.Result.Status == Messaging.PublicationResultStatus.Published);
				Console.WriteLine(string.Format("RanToCompletionTaskCount expected(nearly): (10000), actual: ({0})", _taskStatusCount));
				Console.WriteLine(string.Format("MessagePublishedCount expected(nearly): (10000), actual: ({0})", _messagePublishedCount));
			});

			Console.Read();
		}


		public void Publish_Continues_When_Node_We_Are_Connected_To_Has_Connection_Forced_Closed_Via_Management_UI()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder(Accessories.Configuration.ConnectionSettingsString);
			_busConfigurationBuilder
				.RegisterPublication<Accessories.MyEvent>(Accessories.Configuration.ExchangeName1, typeof(Accessories.MyEvent).Name, MessageDeliveryMode.Persistent, message => Accessories.Configuration.QueueName1);
			var _SUT = new Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			// Let node that we are connected to = NodeA
			// Let other node = NodeB

			// Before forcing the connection to be closed on NodeA, verify cluster is in working order
			// We publish continuously for 10,000 messages
			// During publication, force connection closed on NodeA via management UI
			// Verify nearly all 10,000 messages exist on queue
			//	- we expect some messages to go missing here as not all messages may get published when a network failure occurs
			// Verify we reconnect via automatic recovery

			var _tasks = new ConcurrentBag<Task<PMCG.Messaging.PublicationResult>>();
			for (int count = 0; count < 10000; count++)
			{
				var _message = new Accessories.MyEvent(Guid.NewGuid(), "", "R1", 1, "09:00", "DDD....");
				_tasks.Add(_SUT.PublishAsync(_message));
				Console.WriteLine(count);
				Thread.Sleep(1);
			}

			Task.WhenAll(_tasks).ContinueWith(a =>
			{
				var _taskStatusCount = _tasks.Count(result => result.Status == TaskStatus.RanToCompletion);
				var _messagePublishedCount = _tasks.Count(result => result.Result.Status == Messaging.PublicationResultStatus.Published);
				Console.WriteLine(string.Format("RanToCompletionTaskCount expected(nearly): (10000), actual: ({0})", _taskStatusCount));
				Console.WriteLine(string.Format("MessagePublishedCount expected(nearly): (10000), actual: ({0})", _messagePublishedCount));
			});

			Console.Read();
		}


		public void Consume_Continues_When_Node_We_Are_Not_Connected_To_Has_RabbitMQ_Service_Stopped()
		{
			var _publisherBusConfigurationBuilder = new BusConfigurationBuilder(Accessories.Configuration.ConnectionSettingsString.Replace("myTestConnection", "publisher"));
			_publisherBusConfigurationBuilder
				.RegisterPublication<Accessories.MyEvent>(Accessories.Configuration.ExchangeName1, typeof(Accessories.MyEvent).Name, MessageDeliveryMode.Persistent, message => "test.queue.1");
			var _publisherBus = new Bus(_publisherBusConfigurationBuilder.Build());
			_publisherBus.Connect();

			// Let node that we are connected to = NodeA
			// Let other node = NodeB

			// Before stopping rabbitmq service on NodeB, verify cluster is in working order
			// We publish 50,000 messages and verify that these are on the queue
			// We continuously consume the 50,000 messages
			// During consumption, stop rabbitmq service on NodeB
			//		service rabbitmq-server stop
			// Verify 0 messages exist on queue
			// Verify consumed message count is at least 50,000
			// Start rabbitmq service on NodeB

			for (int count = 0; count < 50000; count++)
			{
				var _messageId = Guid.NewGuid();
				var _message = new Accessories.MyEvent(_messageId, null, "R1", 1, "09:00", "DDD....");
				_publisherBus.PublishAsync(_message);
			}

			Console.WriteLine("Verify at least 50000 messages in the queue");
			// Note that we see that a number of messages seem to get delivered more than once. This however is expected behaviour as we are using acknowledgements
			// See link: https://groups.google.com/forum/#!topic/rabbitmq-users/PODU8wIYmQs
			Console.ReadKey();

			var _consumedMessageCount = 0;
			var _consumerBusConfigurationBuilder = new BusConfigurationBuilder(Accessories.Configuration.ConnectionSettingsString.Replace("myTestConnection", "consumer"));
			_consumerBusConfigurationBuilder.NumberOfConsumers = 5;
			_consumerBusConfigurationBuilder
				.RegisterConsumer<Accessories.MyEvent>(
					Accessories.Configuration.QueueName1,
					typeof(Accessories.MyEvent).Name,
					message => {
						Interlocked.Increment(ref _consumedMessageCount);
						return ConsumerHandlerResult.Completed;
					});
			var _consumerBus = new Bus(_consumerBusConfigurationBuilder.Build());
			_consumerBus.Connect();

			for (int _counter = 0; _counter < 60; _counter++)
			{
				Thread.Sleep(1000);
				Console.WriteLine(string.Format("{0} seconds completed", _counter));
			}
			
			Console.WriteLine("Ensure there are no messages in the queue");
			Console.WriteLine(string.Format("Consumed message count expected(at least): (50000), actual: ({0})", _consumedMessageCount));
			Console.ReadKey();
		}


		public void Consume_Continues_When_Node_We_Are_Connected_To_Has_RabbitMQ_Service_Stopped()
		{
			var _publisherBusConfigurationBuilder = new BusConfigurationBuilder(Accessories.Configuration.ConnectionSettingsString.Replace("myTestConnection", "publisher"));
			_publisherBusConfigurationBuilder
				.RegisterPublication<Accessories.MyEvent>(Accessories.Configuration.ExchangeName1, typeof(Accessories.MyEvent).Name, MessageDeliveryMode.Persistent, message => "test.queue.1");
			var _publisherBus = new Bus(_publisherBusConfigurationBuilder.Build());
			_publisherBus.Connect();

			// Let node that we are connected to = NodeA
			// Let other node = NodeB

			// Before stopping rabbitmq service on NodeA, verify cluster is in working order
			// We publish 50,000 messages and verify that these are on the queue
			// We continuously consume the 50,000 messages
			// During consumption, stop rabbitmq service on NodeA
			//		service rabbitmq-server stop
			// Verify 0 messages exist on queue
			// Verify consumed message count is at least 50,000
			// Start rabbitmq service on NodeA

			// Note that sometimes we see that the queue does not failover to a slave due to queues being in an unsynchronized state
			// This will only ever happen if the rabbitmq server is manually stopped. This is not applicable to server reboot for example 
			// that can occur during Ubuntu updates. If you upgrade the cluster in situ (for minor version upgrades), this has a possibility
			// of occurring albeit its highly unlikely. The outcome of this is that the queue does not get back to normality until the node is restarted.
			// See section "Stopping master nodes with only unsynchronised slaves": https://rabbitmq.docs.pivotal.io/36/rabbit-web-docs/ha.html#start-stop

			for (int count = 0; count < 50000; count++)
			{
				var _messageId = Guid.NewGuid();
				var _message = new Accessories.MyEvent(_messageId, null, "R1", 1, "09:00", "DDD....");
				_publisherBus.PublishAsync(_message);
			}

			Console.WriteLine("Verify at least 50000 messages in the queue");
			// Note that we see that a number of messages seem to get delivered more than once. This however is expected behaviour as we are using acknowledgements
			// See link: https://groups.google.com/forum/#!topic/rabbitmq-users/PODU8wIYmQs
			Console.ReadKey();

			var _consumedMessageCount = 0;
			var _consumerBusConfigurationBuilder = new BusConfigurationBuilder(Accessories.Configuration.ConnectionSettingsString.Replace("myTestConnection", "consumer"));
			_consumerBusConfigurationBuilder.NumberOfConsumers = 2;
			_consumerBusConfigurationBuilder
				.RegisterConsumer<Accessories.MyEvent>(
					Accessories.Configuration.QueueName1,
					typeof(Accessories.MyEvent).Name,
					message => {
						Interlocked.Increment(ref _consumedMessageCount);
						return ConsumerHandlerResult.Completed;
					});
			var _consumerBus = new Bus(_consumerBusConfigurationBuilder.Build());
			_consumerBus.Connect();

			for (int _counter = 0; _counter < 60; _counter++)
			{
				Thread.Sleep(1000);
				Console.WriteLine(string.Format("{0} seconds completed", _counter));
			}

			Console.WriteLine("Ensure there are no messages in the queue");
			Console.WriteLine(string.Format("Consumed message count expected(at least): (50000), actual: ({0})", _consumedMessageCount));
			Console.ReadKey();
		}


		public void Consume_Continues_When_Node_We_Are_Not_Connected_To_Is_Rebooted()
		{
			var _publisherBusConfigurationBuilder = new BusConfigurationBuilder(Accessories.Configuration.ConnectionSettingsString.Replace("myTestConnection", "publisher"));
			_publisherBusConfigurationBuilder
				.RegisterPublication<Accessories.MyEvent>(Accessories.Configuration.ExchangeName1, typeof(Accessories.MyEvent).Name, MessageDeliveryMode.Persistent, message => "test.queue.1");
			var _publisherBus = new Bus(_publisherBusConfigurationBuilder.Build());
			_publisherBus.Connect();

			// Let node that we are connected to = NodeA
			// Let other node = NodeB

			// Before rebooting NodeB, verify cluster is in working order
			// We publish 50,000 messages and verify that these are on the queue
			// We continuously consume the 50,000 messages
			// During consumption, reboot NodeB
			// Verify 0 messages exist on queue
			// Verify consumed message count at least 50,000
			
			for (int count = 0; count < 50000; count++)
			{
				var _messageId = Guid.NewGuid();
				var _message = new Accessories.MyEvent(_messageId, null, "R1", 1, "09:00", "DDD....");
				_publisherBus.PublishAsync(_message);
			}

			Console.WriteLine("Verify at least 50000 messages in the queue");
			Console.ReadKey();

			var _consumedMessageCount = 0;
			var _consumerBusConfigurationBuilder = new BusConfigurationBuilder(Accessories.Configuration.ConnectionSettingsString.Replace("myTestConnection", "consumer"));
			_consumerBusConfigurationBuilder.NumberOfConsumers = 5;
			_consumerBusConfigurationBuilder
				.RegisterConsumer<Accessories.MyEvent>(
					Accessories.Configuration.QueueName1,
					typeof(Accessories.MyEvent).Name,
					message => {
						Interlocked.Increment(ref _consumedMessageCount);
						return ConsumerHandlerResult.Completed;
					});
			var _consumerBus = new Bus(_consumerBusConfigurationBuilder.Build());
			_consumerBus.Connect();

			for (int _counter = 0; _counter < 60; _counter++)
			{
				Thread.Sleep(1000);
				Console.WriteLine(string.Format("{0} seconds completed", _counter));
			}

			Console.WriteLine("Ensure there are no messages in the queue");
			Console.WriteLine(string.Format("Consumed message count expected(at least): (50000), actual: ({0})", _consumedMessageCount));
			Console.ReadKey();
		}


		public void Consume_Continues_When_Node_We_Are_Connected_To_Is_Rebooted()
		{
			var _publisherBusConfigurationBuilder = new BusConfigurationBuilder(Accessories.Configuration.ConnectionSettingsString.Replace("myTestConnection", "publisher"));
			_publisherBusConfigurationBuilder
				.RegisterPublication<Accessories.MyEvent>(Accessories.Configuration.ExchangeName1, typeof(Accessories.MyEvent).Name, MessageDeliveryMode.Persistent, message => "test.queue.1");
			var _publisherBus = new Bus(_publisherBusConfigurationBuilder.Build());
			_publisherBus.Connect();

			// Let node that we are connected to = NodeA
			// Let other node = NodeB

			// Before rebooting NodeA, verify cluster is in working order
			// We publish 50,000 messages and verify that these are on the queue
			// We continuously consume the 50,000 messages
			// During consumption, reboot NodeA
			// Verify 0 messages exist on queue
			// Verify consumed message count is nearly 50,000

			for (int count = 0; count < 50000; count++)
			{
				var _messageId = Guid.NewGuid();
				var _message = new Accessories.MyEvent(_messageId, null, "R1", 1, "09:00", "DDD....");
				_publisherBus.PublishAsync(_message);
			}

			Console.WriteLine("Verify at least 50000 messages in the queue");
			Console.ReadKey();

			var _consumedMessageCount = 0;
			var _consumerBusConfigurationBuilder = new BusConfigurationBuilder(Accessories.Configuration.ConnectionSettingsString.Replace("myTestConnection", "consumer"));
			_consumerBusConfigurationBuilder.NumberOfConsumers = 5;
			_consumerBusConfigurationBuilder
				.RegisterConsumer<Accessories.MyEvent>(
					Accessories.Configuration.QueueName1,
					typeof(Accessories.MyEvent).Name,
					message => {
						Interlocked.Increment(ref _consumedMessageCount);
						return ConsumerHandlerResult.Completed;
					});
			var _consumerBus = new Bus(_consumerBusConfigurationBuilder.Build());
			_consumerBus.Connect();

			for (int _counter = 0; _counter < 60; _counter++)
			{
				Thread.Sleep(1000);
				Console.WriteLine(string.Format("{0} seconds completed", _counter));
			}

			Console.WriteLine("Ensure there are no messages in the queue");
			Console.WriteLine(string.Format("Consumed message count expected(nearly): (50000), actual: ({0})", _consumedMessageCount));
			Console.ReadKey();
		}


		public void Consume_Continues_When_Node_We_Are_Not_Connected_To_Is_Blocked()
		{
			var _publisherBusConfigurationBuilder = new BusConfigurationBuilder(Accessories.Configuration.ConnectionSettingsString.Replace("myTestConnection", "publisher"));
			_publisherBusConfigurationBuilder
				.RegisterPublication<Accessories.MyEvent>(Accessories.Configuration.ExchangeName1, typeof(Accessories.MyEvent).Name, MessageDeliveryMode.Persistent, message => "test.queue.1");
			var _publisherBus = new Bus(_publisherBusConfigurationBuilder.Build());
			_publisherBus.Connect();

			// Let node that we are connected to = NodeA
			// Let other node = NodeB

			// Before blocking rabbitmq service on NodeB, verify cluster is in working order
			// We publish 50,000 messages and verify that these are on the queue
			// We continuously consume the 50,000 messages
			// During consumption, block NodeB for a period of time, then unblock
			//		rabbitmqctl set_vm_memory_high_watermark 0.0000001
			//		rabbitmqctl set_vm_memory_high_watermark 0.4
			// Verify 0 messages exist on queue
			// Verify consumed message count is at least 50,000

			for (int count = 0; count < 50000; count++)
			{
				var _messageId = Guid.NewGuid();
				var _message = new Accessories.MyEvent(_messageId, null, "R1", 1, "09:00", "DDD....");
				_publisherBus.PublishAsync(_message);
			}

			Console.WriteLine("Verify exactly 50000 messages in the queue");
			// Note that we see that a number of messages seem to get delivered more than once. This however is expected behaviour as we are using acknowledgements
			// See link: https://groups.google.com/forum/#!topic/rabbitmq-users/PODU8wIYmQs
			Console.ReadKey();

			var _consumedMessageCount = 0;
			var _consumerBusConfigurationBuilder = new BusConfigurationBuilder(Accessories.Configuration.ConnectionSettingsString.Replace("myTestConnection", "consumer"));
			_consumerBusConfigurationBuilder.NumberOfConsumers = 5;
			_consumerBusConfigurationBuilder
				.RegisterConsumer<Accessories.MyEvent>(
					Accessories.Configuration.QueueName1,
					typeof(Accessories.MyEvent).Name,
					message => {
						Interlocked.Increment(ref _consumedMessageCount);
						return ConsumerHandlerResult.Completed;
					});
			var _consumerBus = new Bus(_consumerBusConfigurationBuilder.Build());
			_consumerBus.Connect();

			for (int _counter = 0; _counter < 60; _counter++)
			{
				Thread.Sleep(1000);
				Console.WriteLine(string.Format("{0} seconds completed", _counter));
			}

			Console.WriteLine("Ensure there are no messages in the queue");
			Console.WriteLine(string.Format("Consumed message count expected(at least): (50000), actual: ({0})", _consumedMessageCount));
			Console.ReadKey();
		}


		public void Consume_Continues_When_Node_We_Are_Connected_To_Is_Blocked()
		{
			var _publisherBusConfigurationBuilder = new BusConfigurationBuilder(Accessories.Configuration.ConnectionSettingsString.Replace("myTestConnection", "publisher"));
			_publisherBusConfigurationBuilder
				.RegisterPublication<Accessories.MyEvent>(Accessories.Configuration.ExchangeName1, typeof(Accessories.MyEvent).Name, MessageDeliveryMode.Persistent, message => "test.queue.1");
			var _publisherBus = new Bus(_publisherBusConfigurationBuilder.Build());
			_publisherBus.Connect();

			// Let node that we are connected to = NodeA
			// Let other node = NodeB

			// Before blocking rabbitmq service on NodeB, verify cluster is in working order
			// We publish 50,000 messages and verify that these are on the queue
			// We continuously consume the 50,000 messages
			// During consumption, block NodeA for a period of time, then unblock
			//		ssh ccs\whoever_admin@NodeA.ccs.local
			//		rabbitmqctl set_vm_memory_high_watermark 0.0000001
			//		rabbitmqctl set_vm_memory_high_watermark 0.4
			// Verify 0 messages exist on queue
			// Verify consumed message count is at least 50,000

			for (int count = 0; count < 50000; count++)
			{
				var _messageId = Guid.NewGuid();
				var _message = new Accessories.MyEvent(_messageId, null, "R1", 1, "09:00", "DDD....");
				_publisherBus.PublishAsync(_message);
			}

			Console.WriteLine("Verify exactly 50000 messages in the queue");
			// Note that we see that a number of messages seem to get delivered more than once. This however is expected behaviour as we are using acknowledgements
			// See link: https://groups.google.com/forum/#!topic/rabbitmq-users/PODU8wIYmQs
			Console.ReadKey();

			var _consumedMessageCount = 0;
			var _consumerBusConfigurationBuilder = new BusConfigurationBuilder(Accessories.Configuration.ConnectionSettingsString.Replace("myTestConnection", "consumer"));
			_consumerBusConfigurationBuilder.NumberOfConsumers = 5;
			_consumerBusConfigurationBuilder
				.RegisterConsumer<Accessories.MyEvent>(
					Accessories.Configuration.QueueName1,
					typeof(Accessories.MyEvent).Name,
					message => {
						Interlocked.Increment(ref _consumedMessageCount);
						return ConsumerHandlerResult.Completed;
					});
			var _consumerBus = new Bus(_consumerBusConfigurationBuilder.Build());
			_consumerBus.Connect();

			for (int _counter = 0; _counter < 60; _counter++)
			{
				Thread.Sleep(1000);
				Console.WriteLine(string.Format("{0} seconds completed", _counter));
			}

			Console.WriteLine("Ensure there are no messages in the queue");
			Console.WriteLine(string.Format("Consumed message count expected(at least): (50000), actual: ({0})", _consumedMessageCount));
			Console.ReadKey();
		}


		public void Consume_Continues_When_Node_We_Are_Connected_To_Has_Connection_Forced_Closed_Via_Management_UI()
		{
			var _publisherBusConfigurationBuilder = new BusConfigurationBuilder(Accessories.Configuration.ConnectionSettingsString.Replace("myTestConnection", "publisher"));
			_publisherBusConfigurationBuilder
				.RegisterPublication<Accessories.MyEvent>(Accessories.Configuration.ExchangeName1, typeof(Accessories.MyEvent).Name, MessageDeliveryMode.Persistent, message => "test.queue.1");
			var _publisherBus = new Bus(_publisherBusConfigurationBuilder.Build());
			_publisherBus.Connect();

			// Let node that we are connected to = NodeA
			// Let other node = NodeB

			// Before forcing the connection to be closed on NodeA, verify cluster is in working order
			// We publish 50,000 messages and verify that these are on the queue
			// We continuously consume the 50,000 messages
			// During consumption, force connection closed on NodeA via management UI
			// Verify 0 messages exist on queue
			// Verify consumed message count is at least 50,000

			for (int count = 0; count < 50000; count++)
			{
				var _messageId = Guid.NewGuid();
				var _message = new Accessories.MyEvent(_messageId, null, "R1", 1, "09:00", "DDD....");
				_publisherBus.PublishAsync(_message);
			}

			Console.WriteLine("Verify exactly 50000 messages in the queue");
			// Note that we see that a number of messages seem to get delivered more than once. This however is expected behaviour as we are using acknowledgements
			// See link: https://groups.google.com/forum/#!topic/rabbitmq-users/PODU8wIYmQs
			Console.ReadKey();

			var _consumedMessageCount = 0;
			var _consumerBusConfigurationBuilder = new BusConfigurationBuilder(Accessories.Configuration.ConnectionSettingsString.Replace("myTestConnection", "consumer"));
			_consumerBusConfigurationBuilder.NumberOfConsumers = 5;
			_consumerBusConfigurationBuilder
				.RegisterConsumer<Accessories.MyEvent>(
					Accessories.Configuration.QueueName1,
					typeof(Accessories.MyEvent).Name,
					message => {
						Interlocked.Increment(ref _consumedMessageCount);
						return ConsumerHandlerResult.Completed;
					});
			var _consumerBus = new Bus(_consumerBusConfigurationBuilder.Build());
			_consumerBus.Connect();

			for (int _counter = 0; _counter < 60; _counter++)
			{
				Thread.Sleep(1000);
				Console.WriteLine(string.Format("{0} seconds completed", _counter));
			}

			Console.WriteLine("Ensure there are no messages in the queue");
			Console.WriteLine(string.Format("Consumed message count expected(at least): (50000), actual: ({0})", _consumedMessageCount));
			Console.ReadKey();
		}


		public void Reboot_Both_Nodes_In_The_Cluster_Simultaneously()
		{
			// Let node that queues were created on = NodeA
			// Let other node = NodeB

			// Before reboot of NodeA and NodeB, verify cluster is in working order
			// Reboot NodeA and NodeB as simultaneously as possible
			// After successful reboot, verify that cluster is restored back to working order
			// Verify that all exchanges, queues etc are all as expected
		}


		public void Kill_Both_Nodes_VM_In_The_Cluster_Simultaneously()
		{
			// Let node that queues were created on = NodeA
			// Let other node = NodeB

			// Before reboot of NodeA and NodeB, verify cluster is in working order
			// Turn off NodeA and NodeB simultaneously (you need a member of IT services to execute this task via VMWare manager)
			// After successful reboot, verify that cluster is restored back to working order
			// Verify that all exchanges, queues etc are all as expected
		}
	}
}