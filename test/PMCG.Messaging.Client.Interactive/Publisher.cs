using PMCG.Messaging.Client.Configuration;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;


namespace PMCG.Messaging.Client.Interactive
{
	public class Publisher
	{
		private IConnection c_connection;
		private BlockingCollection<PMCG.Messaging.Client.Publication> c_publicationQueue;
		private PMCG.Messaging.Client.Publisher c_publisher;


		public void Run_Where_We_Instruct_To_Stop_The_Broker()
		{
			this.InstantiateAndStartPublisher();

			Console.WriteLine("Stop the broker by running the following command as an admin");
			Console.WriteLine("\t rabbitmqctl.bat stop");
			Console.WriteLine("After stopping the broker hit enter to exit");
			Console.ReadLine();
		}


		public void Run_Where_We_Close_The_Connection_Using_The_DashBoard()
		{
			this.InstantiateAndStartPublisher();

			Console.WriteLine("Close the connection from the dashboard");
			Console.WriteLine("After closing the connecton hit enter to exit");
			Console.ReadLine();
		}


		public void Run_Where_We_Publish_A_Message_To_A_Non_Existent_Exchange_Will_Close_The_Internal_Channel()
		{
			this.InstantiateAndStartPublisher();

			Console.WriteLine("Hit enter to publish");
			Console.ReadLine();

			var _publication = new Publication(
				new MessageDelivery("NON_EXISTENT_EXCHANGE", "H", MessageDeliveryMode.Persistent, m => "Ted"),
				new MyEvent(Guid.NewGuid(), "", "R1", 1, "09:00", "DDD...."),
				new TaskCompletionSource<PublicationResult>());
			this.c_publicationQueue.Add(_publication);

			try
			{
				_publication.ResultTask.Wait();
			}
			catch (AggregateException exception)
			{
				Console.WriteLine("Exception - should be 404 channel sutdown - {0}", exception.InnerExceptions[0].Message);
			}

			Console.WriteLine("Hit enter to close connection (Channel should already be closed - check the dashboard)");
			Console.ReadLine();
			this.c_connection.Close();

			Console.WriteLine("Hit enter to exit");
			Console.ReadLine();
		}


		public void InstantiateAndStartPublisher()
		{
			this.c_connection = new ConnectionFactory { Uri = new Uri(Configuration.LocalConnectionUri) }.CreateConnection();
			this.c_publicationQueue = new BlockingCollection<PMCG.Messaging.Client.Publication>();

			this.c_publisher = new PMCG.Messaging.Client.Publisher(this.c_connection, this.c_publicationQueue);
			this.c_publisher.Start();
		}
	}
}
