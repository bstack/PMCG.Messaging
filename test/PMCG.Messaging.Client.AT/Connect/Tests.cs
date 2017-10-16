using PMCG.Messaging.Client.Configuration;
using System;


namespace PMCG.Messaging.Client.AT.Connect
{
	public class Tests
	{
		public void Connect_Non_Existent_Broker_Indefinitely()
		{
			var _connectionSettingsString = Accessories.Configuration.ConnectionSettingsString.Replace("5672", "2567"); // Wrong port number
			var _busConfigurationBuilder = new BusConfigurationBuilder(_connectionSettingsString);
			_busConfigurationBuilder.ConnectionClientProvidedName = Accessories.Configuration.ConnectionClientProvidedName;
			var _SUT = new Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			Console.WriteLine("Allow time for connection attempts to fail, should see retries indefinitely");
		}


		public void Connect_Restart_Broker_Connection_Reestablished_Automatically()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder(Accessories.Configuration.ConnectionSettingsString);
			_busConfigurationBuilder.ConnectionClientProvidedName = Accessories.Configuration.ConnectionClientProvidedName;
			var _SUT = new Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			Console.WriteLine(@"Stop the broker by running the following command '.\rabbitmqctl.bat stop'");
			Console.WriteLine(@"Start the broker by running the following command '.\rabbitmq-server.bat -detached'");
			Console.WriteLine("Verify connection in management ui is re-established automatically via automatic recovery");
			Console.Read();
		}


		public void Connect_Close_The_Connection_Using_The_Management_UI_Connection_Reestablished_Automatically()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder(Accessories.Configuration.ConnectionSettingsString);
			_busConfigurationBuilder.ConnectionClientProvidedName = Accessories.Configuration.ConnectionClientProvidedName;

			var _SUT = new Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			Console.WriteLine("Close the connection from the management ui");
			Console.WriteLine("Verify connection in management ui is re-established automatically via automatic recovery ");
			Console.Read();
		}


		public void Connect_Is_Already_Started_Then_Blocked_And_Then_Unblocked()
		{
			var _busConfigurationBuilder = new BusConfigurationBuilder(Accessories.Configuration.ConnectionSettingsString);
			_busConfigurationBuilder.ConnectionClientProvidedName = Accessories.Configuration.ConnectionClientProvidedName;

			var _SUT = new Bus(_busConfigurationBuilder.Build());
			_SUT.Connect();

			Console.WriteLine("Block the broker by running the following command");
			Console.WriteLine(@".\rabbitmqctl.bat set_vm_memory_high_watermark 0.0000001");
			Console.WriteLine("Verify connection state is blocked");
			Console.Read();

			Console.WriteLine("Unblock the broker by running the following command");
			Console.WriteLine("\t .\rabbitmqctl.bat set_vm_memory_high_watermark 0.4");
			Console.Read();
			Console.WriteLine("Verify connection state is running");
			Console.Read();
		}
	}
}