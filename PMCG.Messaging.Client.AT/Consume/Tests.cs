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
		public void Publish_A_Message_And_Consume_For_The_Same_Messsage_With_Ack()
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


		public void Publish_A_Message_And_Consume_For_The_Same_Messsage_With_Nack()
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


		public void Publish_A_Message_And_Consume_For_The_Same_Messsage_With_Nack_And_Dead_Letter_Queue()
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


		public void Publish_1000_Messages_And_Consume_For_The_Same_Messsages_With_Ack()
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


		public void Publish_1000_Messages_And_Consume_For_The_Same_Messsages_With_Nack()
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


		public void Publish_1000_Messages_And_Consume_For_The_Same_Messsages_With_Half_Acked_Half_Nacked()
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



















		public void Publish_A_Message_To_A_Queue_Using_The_Direct_Exchange()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(Accessories.Configuration.LocalConnectionUri);
			_busConfigurationBuilder.ConnectionClientProvidedName = Accessories.Configuration.ConnectionClientProvidedName;
			_busConfigurationBuilder
				.RegisterPublication<Accessories.MyEvent>("", typeof(Accessories.MyEvent).Name, MessageDeliveryMode.Persistent, message => Accessories.Configuration.QueueName1)
				.RegisterConsumer<Accessories.MyEvent>(
					typeof(Accessories.MyEvent).Name,
					message => { return ConsumerHandlerResult.Completed; },
				Accessories.Configuration.ExchangeName1);

			var _SUT = new PMCG.Messaging.Client.Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			var _message = new Accessories.MyEvent(Guid.NewGuid(), "", "R1", 1, "09:00", "DDD....");
			var _result = _SUT.PublishAsync(_message);
			_result.Wait(TimeSpan.FromSeconds(1));

			Console.WriteLine(string.Format("TaskStatus expected: (RanToCompletion), actual: ({0})", _result.Status));
			Console.WriteLine(string.Format("PublicationResultStatus expected: (Published), actual: ({0})", _result.Result.Status));
			Console.Read();
		}


		public void Publish_A_Message_To_A_Queue_Using_Custom_Exchange()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(Accessories.Configuration.LocalConnectionUri);
			_busConfigurationBuilder.ConnectionClientProvidedName = Accessories.Configuration.ConnectionClientProvidedName;
			_busConfigurationBuilder
				.RegisterPublication<Accessories.MyEvent>(Accessories.Configuration.ExchangeName1, typeof(Accessories.MyEvent).Name, MessageDeliveryMode.Persistent, message => Accessories.Configuration.QueueName1);
			var _SUT = new PMCG.Messaging.Client.Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			var _message = new Accessories.MyEvent(Guid.NewGuid(), "", "R1", 1, "09:00", "DDD....");
			var _result = _SUT.PublishAsync(_message);
			_result.Wait(TimeSpan.FromSeconds(1));

			Console.WriteLine(string.Format("TaskStatus expected: (RanToCompletion), actual: ({0})", _result.Status));
			Console.WriteLine(string.Format("PublicationResultStatus expected: (Published), actual: ({0})", _result.Result.Status));
			Console.Read();
		}
	
	}
}
