using PMCG.Messaging.Client.Configuration;
using RabbitMQ.Client;
using System;
using System.Threading;
using System.Threading.Tasks;


namespace PMCG.Messaging.Client.Interactive
{
	public class Consumers
	{
		private int c_numberOfConsumers = 2;
		private IConnection c_connection;


		public void Run_Where_We_Instruct_To_Stop_The_Broker()
		{
			this.InstantiateConsumers();

			Console.WriteLine("Stop the broker by running the following command as an admin");
			Console.WriteLine("\t rabbitmqctl.bat stop");
			Console.WriteLine("After stopping the broker hit enter to exit");
			Console.ReadLine();
		}


		public void Run_Where_We_Close_The_Connection_Using_The_DashBoard()
		{
			this.InstantiateConsumers();

			Console.WriteLine("Close the connection from the dashboard");
			Console.WriteLine("After closing the connecton hit enter to exit");
			Console.ReadLine();
		}


		public void InstantiateConsumers()
		{
			var _connectionUri = Configuration.LocalConnectionUri;
			var _connectionFactory = new ConnectionFactory
			{
				Uri = new Uri(_connectionUri),
				UseBackgroundThreadsForIO = false,
				AutomaticRecoveryEnabled = true,
				TopologyRecoveryEnabled = true
			};
			this.c_connection = _connectionFactory.CreateConnection();

			var _busConfigurationBuilder = new BusConfigurationBuilder();
			_busConfigurationBuilder.ConnectionUris.Add(_connectionUri);

			for(var _index = 0; _index < this.c_numberOfConsumers; _index++)
			{
				var _consumer = new PMCG.Messaging.Client.Consumer(this.c_connection, _busConfigurationBuilder.Build());
				_consumer.Start();
			}
		}
	}
}