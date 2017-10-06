using System;

namespace PMCG.Messaging.Client.Interactive
{
	public class ConnectionManager
	{
		public void Run_Open()
		{
			Console.WriteLine("Start the broker by running the following command as an admin");
			Console.WriteLine("\t .\rabbitmq-server.bat -detached");

			var _SUT = new PMCG.Messaging.Client.ConnectionManager(
				new[] { Configuration.LocalConnectionUri },
				Configuration.ConnectionClientProvidedName,
				TimeSpan.FromSeconds(4));

			_SUT.Open();
			Console.WriteLine(string.Format("Is Connection open: {0}", _SUT.IsOpen));
		}


		public void Run_Open_Where_Server_Is_Already_Stopped_And_Instruct_To_Start_Server()
		{
			Console.WriteLine("Stop the broker by running the following command as an admin");
			Console.WriteLine("\t .\rabbitmqctl.bat stop");

			var _SUT = new PMCG.Messaging.Client.ConnectionManager(
				new[] { Configuration.LocalConnectionUri },
				Configuration.ConnectionClientProvidedName,
				TimeSpan.FromSeconds(4));

			_SUT.Open();

			Console.WriteLine("Start the broker by running the following command as an admin");
			Console.WriteLine("\t .\rabbitmq-server.bat -detached");

			Console.WriteLine(string.Format("Is Connection open: {0}", _SUT.IsOpen));
		}


		public void Run_Open_Where_Server_Is_Already_Started_Then_Blocked_And_Then_Unblocked()
		{
			var _SUT = new PMCG.Messaging.Client.ConnectionManager(
				new[] { Configuration.LocalConnectionUri },
				Configuration.ConnectionClientProvidedName,
				TimeSpan.FromSeconds(4));

			_SUT.Open();
			Console.WriteLine(string.Format("Is Connection open: {0}", _SUT.IsOpen));

			Console.WriteLine("Block the broker by running the following command as an admin");
			Console.WriteLine("\t .\rabbitmqctl.bat set_vm_memory_high_watermark 0.0000001");
			Console.Read();
			Console.WriteLine(string.Format("Is Connection open: {0}", _SUT.IsOpen));

			Console.WriteLine("Unblock the broker by running the following command as an admin");
			Console.WriteLine("\t .\rabbitmqctl.bat set_vm_memory_high_watermark 0.4");
			Console.Read();
			Console.WriteLine(string.Format("Is Connection open: {0}", _SUT.IsOpen));
		}
	}
}
