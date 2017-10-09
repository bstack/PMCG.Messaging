using log4net;
using log4net.Config;
using System;
using System.Diagnostics;


namespace PMCG.Messaging.Client.AT
{
	public class Program
	{
		static void Main(
			string[] args)
		{
			XmlConfigurator.Configure();
			GlobalContext.Properties["pid"] = Process.GetCurrentProcess().Id;   // See http://stackoverflow.com/questions/2075603/log4net-process-id-information

			//new PMCG.Messaging.Client.AT.Publish.Tests().Publish_A_Message_To_A_Queue_Using_The_Direct_Exchange();
			//new PMCG.Messaging.Client.AT.Publish.Tests().Publish_A_Message_To_A_Queue_Using_Custom_Exchange();
			//new PMCG.Messaging.Client.AT.Publish.Tests().Publish_A_Message_To_An_Exchange_That_Doesnt_Exist();
			//new PMCG.Messaging.Client.AT.Publish.Tests().Publish_1000_Messages_To_A_Queue_Using_Custom_Exchange();
			//new PMCG.Messaging.Client.AT.Publish.Tests().Publish_With_Timeout();
			//new PMCG.Messaging.Client.AT.Publish.Tests().Publish_Connection_Closed_By_Application_Never_Recovers();
			//new PMCG.Messaging.Client.AT.Publish.Tests().Publish_Connection_Closed_By_Server_Recovers_Automatically();
			//new PMCG.Messaging.Client.AT.Publish.Tests().Publish_Connection_Closed_By_Server_Restart_Unpublished_Messages_Are_Republished_Successfully();
			new PMCG.Messaging.Client.AT.Publish.Tests().Publish_Connection_Blocked_Then_Unblocked_Unpublished_Messages_Are_Republished_Successfully();
		}
	}
}
