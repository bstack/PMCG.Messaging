using PMCG.Messaging.Client.Configuration;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace PMCG.Messaging.Client.AT.Consume
{
	public class Tests
	{
		public void Publish_A_Message_And_Consume_For_The_Same_Message_With_Ack()
		{
			var _capturedMessageId = string.Empty;

			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(Accessories.Configuration.LocalConnectionUri);
			_busConfigurationBuilder.ConnectionClientProvidedName = "testconnectionname";
			_busConfigurationBuilder.NumberOfConsumers = 2;
			_busConfigurationBuilder
				.RegisterPublication<Accessories.MyEvent>(Accessories.Configuration.ExchangeName1, typeof(Accessories.MyEvent).Name)
				.RegisterConsumer<Accessories.MyEvent>(
					Accessories.Configuration.QueueName1,
					typeof(Accessories.MyEvent).Name,
					message => { _capturedMessageId = message.Id.ToString(); return ConsumerHandlerResult.Completed; });
			var _SUT = new PMCG.Messaging.Client.Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			var _messageId = Guid.NewGuid();
			var _message = new Accessories.MyEvent(_messageId, null, "R1", 1, "09:00", "DDD....");
			_SUT.PublishAsync(_message);
			Thread.Sleep(2000);

			Console.WriteLine(string.Format("Captured message id expected: ({0}), actual: ({1})", _messageId, _capturedMessageId));
			Console.WriteLine(string.Format("Ensure message with id: ({0}) no longer exists in the queue", _messageId));
			Console.Read();
		}


		public void Publish_A_Message_And_Consume_For_The_Same_Message_With_Nack()
		{
			var _capturedMessageId = string.Empty;

			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(Accessories.Configuration.LocalConnectionUri);
			_busConfigurationBuilder.ConnectionClientProvidedName = "testconnectionname";
			_busConfigurationBuilder.NumberOfConsumers = 2;
			_busConfigurationBuilder
				.RegisterPublication<Accessories.MyEvent>(Accessories.Configuration.ExchangeName1, typeof(Accessories.MyEvent).Name)
				.RegisterConsumer<Accessories.MyEvent>(
					Accessories.Configuration.QueueName1,
					typeof(Accessories.MyEvent).Name,
					message => { _capturedMessageId = message.Id.ToString(); return ConsumerHandlerResult.Errored; });
			var _SUT = new PMCG.Messaging.Client.Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			var _messageId = Guid.NewGuid();
			var _message = new Accessories.MyEvent(_messageId, null, "R1", 1, "09:00", "DDD....");
			_SUT.PublishAsync(_message);
			Thread.Sleep(2000);

			Console.WriteLine(string.Format("Captured message id expected: ({0}), actual: ({1})", _messageId, _capturedMessageId));
			Console.WriteLine(string.Format("Ensure message with id: ({0}) no longer exists in the queue", _messageId));
			Console.Read();
		}


		public void Publish_A_Message_And_Consume_For_The_Same_Message_With_Nack_And_Dead_Letter_Queue()
		{
			var _capturedMessageId = string.Empty;

			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(Accessories.Configuration.LocalConnectionUri);
			_busConfigurationBuilder.ConnectionClientProvidedName = "testconnectionname";
			_busConfigurationBuilder.NumberOfConsumers = 2;
			_busConfigurationBuilder
				.RegisterPublication<Accessories.MyEvent>(Accessories.Configuration.ExchangeName2, typeof(Accessories.MyEvent).Name, MessageDeliveryMode.Persistent, message => "test.queue.2")
				.RegisterConsumer<Accessories.MyEvent>(
					Accessories.Configuration.QueueName2,
					typeof(Accessories.MyEvent).Name,
					message => { _capturedMessageId = message.Id.ToString(); return ConsumerHandlerResult.Errored; });
			var _SUT = new PMCG.Messaging.Client.Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			var _messageId = Guid.NewGuid();
			var _message = new Accessories.MyEvent(_messageId, null, "R1", 1, "09:00", "DDD....");
			_SUT.PublishAsync(_message);
			Thread.Sleep(2000);

			Console.WriteLine(string.Format("Captured message id expected: ({0}), actual: ({1})", _messageId, _capturedMessageId));
			Console.WriteLine(string.Format("Ensure message with id: ({0}) no longer exists in the queue", _messageId));
			Console.WriteLine(string.Format("Ensure message with id: ({0}) exists in the dead letter queue - rejected", _messageId));
			Console.Read();
		}


		public void Publish_1000_Messages_And_Consume_For_The_Same_Messages_With_Ack()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(Accessories.Configuration.LocalConnectionUri);
			_busConfigurationBuilder.ConnectionClientProvidedName = "testconnectionname";
			_busConfigurationBuilder.NumberOfConsumers = 2;
			_busConfigurationBuilder
				.RegisterPublication<Accessories.MyEvent>(Accessories.Configuration.ExchangeName2, typeof(Accessories.MyEvent).Name, MessageDeliveryMode.Persistent, message => "test.queue.2")
				.RegisterConsumer<Accessories.MyEvent>(
					Accessories.Configuration.QueueName2,
					typeof(Accessories.MyEvent).Name,
					message => { return ConsumerHandlerResult.Completed; });
			var _SUT = new PMCG.Messaging.Client.Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			for (int count = 0; count < 1000; count++)
			{
				var _messageId = Guid.NewGuid();
				var _message = new Accessories.MyEvent(_messageId, null, "R1", 1, "09:00", "DDD....");
				_SUT.PublishAsync(_message);
			}

			Console.WriteLine("Ensure there are no messages in the queue");
			Console.WriteLine("Ensure there are no messages in the dead letter queue");
			Console.Read();
		}


		public void Publish_1000_Messages_And_Consume_For_The_Same_Messages_With_Nack()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(Accessories.Configuration.LocalConnectionUri);
			_busConfigurationBuilder.ConnectionClientProvidedName = "testconnectionname";
			_busConfigurationBuilder.NumberOfConsumers = 2;
			_busConfigurationBuilder
				.RegisterPublication<Accessories.MyEvent>(Accessories.Configuration.ExchangeName2, typeof(Accessories.MyEvent).Name, MessageDeliveryMode.Persistent, message => "test.queue.2")
				.RegisterConsumer<Accessories.MyEvent>(
					Accessories.Configuration.QueueName2,
					typeof(Accessories.MyEvent).Name,
					message => { return ConsumerHandlerResult.Errored; });
			var _SUT = new PMCG.Messaging.Client.Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			for (int count = 0; count < 1000; count++)
			{
				var _messageId = Guid.NewGuid();
				var _message = new Accessories.MyEvent(_messageId, null, "R1", 1, "09:00", "DDD....");
				_SUT.PublishAsync(_message);
			}

			Console.WriteLine("Ensure there are no messages in the queue");
			Console.WriteLine("Ensure 1000 messages exists in the dead letter queue - rejected");
			Console.Read();
		}


		public void Publish_1000_Messages_And_Consume_For_The_Same_Messages_With_Half_Acked_Half_Nacked()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(Accessories.Configuration.LocalConnectionUri);
			_busConfigurationBuilder.ConnectionClientProvidedName = "testconnectionname";
			_busConfigurationBuilder.NumberOfConsumers = 2;
			_busConfigurationBuilder
				.RegisterPublication<Accessories.MyEvent>(Accessories.Configuration.ExchangeName2, typeof(Accessories.MyEvent).Name, MessageDeliveryMode.Persistent, message => "test.queue.2")
				.RegisterConsumer<Accessories.MyEvent>(
					Accessories.Configuration.QueueName2,
					typeof(Accessories.MyEvent).Name,
					message => { return bool.Parse(message.RunIdentifier) ? ConsumerHandlerResult.Completed : ConsumerHandlerResult.Errored; });
			var _SUT = new PMCG.Messaging.Client.Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			for (int count = 0; count < 1000; count++)
			{
				var _messageId = Guid.NewGuid();
				var _runIdentifier = (count % 2 == 0);
				var _message = new Accessories.MyEvent(_messageId, null, _runIdentifier.ToString(), 1, "09:00", "DDD....");
				_SUT.PublishAsync(_message);
			}

			Console.WriteLine("Ensure there are no messages in the queue");
			Console.WriteLine("Ensure 500 messages exists in the dead letter queue - rejected");
			Console.Read();
		}


		public void Publish_10000_Messages_And_Consume_For_The_Same_Messages_With_Ack_Blocked_Then_Unblocked()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(Accessories.Configuration.LocalConnectionUri);
			_busConfigurationBuilder.ConnectionClientProvidedName = "testconnectionname";
			_busConfigurationBuilder.NumberOfConsumers = 2;
			_busConfigurationBuilder
				.RegisterPublication<Accessories.MyEvent>(Accessories.Configuration.ExchangeName2, typeof(Accessories.MyEvent).Name, MessageDeliveryMode.Persistent, message => "test.queue.2")
				.RegisterConsumer<Accessories.MyEvent>(
					Accessories.Configuration.QueueName2,
					typeof(Accessories.MyEvent).Name,
					message => { return ConsumerHandlerResult.Completed; });
			var _SUT = new PMCG.Messaging.Client.Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			for (int count = 0; count < 10000; count++)
			{
				if (count == 300)
				{
					Console.WriteLine("Set high water mark to very low value (i.e. block)");
					Console.WriteLine("\t rabbitmqctl.bat set_vm_memory_high_watermark 0.0000001");
					Console.ReadKey();
				}

				if (count == 9800)
				{
					Console.WriteLine("Set high water mark to very low value (i.e. unblock)");
					Console.WriteLine("\t rabbitmqctl.bat set_vm_memory_high_watermark 0.4");
					Console.ReadKey();
				}

				var _messageId = Guid.NewGuid();
				var _message = new Accessories.MyEvent(_messageId, null, "R1", 1, "09:00", "DDD....");
				_SUT.PublishAsync(_message);
			}


			Console.WriteLine("Ensure there are no messages in the queue");
			Console.WriteLine("Ensure there are no messages in the dead letter queue");
			Console.ReadKey();
		}


		public void Publish_10000_Messages_And_Consume_For_The_Same_Messages_With_Ack_Connection_Closed_By_Server_Recovers_Automatically()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(Accessories.Configuration.LocalConnectionUri);
			_busConfigurationBuilder.ConnectionClientProvidedName = "testconnectionname";
			_busConfigurationBuilder.NumberOfConsumers = 2;
			_busConfigurationBuilder
				.RegisterPublication<Accessories.MyEvent>(Accessories.Configuration.ExchangeName2, typeof(Accessories.MyEvent).Name, MessageDeliveryMode.Persistent, message => "test.queue.2")
				.RegisterConsumer<Accessories.MyEvent>(
					Accessories.Configuration.QueueName2,
					typeof(Accessories.MyEvent).Name,
					message => { return ConsumerHandlerResult.Completed; });
			var _SUT = new PMCG.Messaging.Client.Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			Console.WriteLine("Close connection via rabbitmq management ui while loop is executing");

			for (int count = 0; count < 10000; count++)
			{
				var _messageId = Guid.NewGuid();
				var _message = new Accessories.MyEvent(_messageId, null, "R1", 1, "09:00", "DDD....");
				_SUT.PublishAsync(_message);
			}


			Console.WriteLine("Ensure there are no messages in the queue");
			Console.WriteLine("Ensure there are no messages in the dead letter queue");
			Console.ReadKey();
		}


		public void Publish_10000_Messages_And_Consume_On_Separate_Bus_For_The_Same_Messages_Consumer_Connection_Closed_By_Server_Recovers_Automatically()
		{
			var _publisherBusConfigurationBuilder = new BusConfigurationBuilder();
			_publisherBusConfigurationBuilder.ConnectionUris.Add(Accessories.Configuration.LocalConnectionUri);
			_publisherBusConfigurationBuilder.ConnectionClientProvidedName = "publisher";
			_publisherBusConfigurationBuilder
				.RegisterPublication<Accessories.MyEvent>(Accessories.Configuration.ExchangeName2, typeof(Accessories.MyEvent).Name, MessageDeliveryMode.Persistent, message => "test.queue.2");
			var _publisherBus = new PMCG.Messaging.Client.Bus(_publisherBusConfigurationBuilder.Build());
			_publisherBus.Connect();

			var _consumerBusConfigurationBuilder = new BusConfigurationBuilder();
			_consumerBusConfigurationBuilder.ConnectionUris.Add(Accessories.Configuration.LocalConnectionUri);
			_consumerBusConfigurationBuilder.ConnectionClientProvidedName = "consumer";
			_consumerBusConfigurationBuilder.NumberOfConsumers = 2;
			_consumerBusConfigurationBuilder
				.RegisterConsumer<Accessories.MyEvent>(
					Accessories.Configuration.QueueName2,
					typeof(Accessories.MyEvent).Name,
					message => { return ConsumerHandlerResult.Completed; });
			var _consumerBus = new PMCG.Messaging.Client.Bus(_consumerBusConfigurationBuilder.Build());
			_consumerBus.Connect();

			for (int count = 0; count < 10000; count++)
			{
				if (count == 1000)
				{
					Console.WriteLine("Close consumer connection via rabbitmq management ui");
					Console.ReadKey();
				}

				var _messageId = Guid.NewGuid();
				var _message = new Accessories.MyEvent(_messageId, null, "R1", 1, "09:00", "DDD....");
				_publisherBus.PublishAsync(_message);
			}

			Console.WriteLine("Ensure there are no messages in the queue");
			Console.WriteLine("Ensure there are no messages in the dead letter queue");
			Console.ReadKey();
		}



		public void Publish_100_Messages_And_Consume_For_The_Same_Messsage_On_A_Transient_Queue()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(Accessories.Configuration.LocalConnectionUri);
			_busConfigurationBuilder.ConnectionClientProvidedName = "testconnectionname";
			_busConfigurationBuilder
				.RegisterPublication<Accessories.MyEvent>(Accessories.Configuration.ExchangeName1, typeof(Accessories.MyEvent).Name)
				.RegisterConsumer<Accessories.MyEvent>(
					typeof(Accessories.MyEvent).Name,
					message => { return ConsumerHandlerResult.Completed; },
					Accessories.Configuration.ExchangeName1);
			var _SUT = new PMCG.Messaging.Client.Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			Console.WriteLine("Wait for transient queue to appear on management ui");
			Console.ReadKey();

			var _message = new Accessories.MyEvent(Guid.NewGuid(), "Correlation Id", "R1", 1, "09:00", "....");
			for (int count = 0; count < 1000; count++)
			{
				_SUT.PublishAsync(_message);
			}

			Console.WriteLine("Ensure there are no messages in the queue");
			Console.WriteLine("Ensure transient queue appears and disappears");
			Console.ReadKey();
		}
	}
}
