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
			new PMCG.Messaging.Client.AT.Publish.Tests().Publish_Connection_Closed_By_Server_Recovers_Automatically();
			//new PMCG.Messaging.Client.AT.Publish.Tests().Publish_Connection_Closed_By_Server_Restart_Unpublished_Messages_Are_Republished_Successfully();
			//new PMCG.Messaging.Client.AT.Publish.Tests().Publish_Connection_Blocked_Then_Unblocked_Unpublished_Messages_Are_Republished_Successfully();
			//new PMCG.Messaging.Client.AT.Publish.Tests().Publish_A_Message_That_Expires_Ends_Up_In_Dead_Letter_Queue();

			//new PMCG.Messaging.Client.AT.Consume.Tests().Publish_A_Message_And_Consume_For_The_Same_Message_With_Ack();
			//new PMCG.Messaging.Client.AT.Consume.Tests().Publish_A_Message_And_Consume_For_The_Same_Message_With_Nack();
			//new PMCG.Messaging.Client.AT.Consume.Tests().Publish_A_Message_And_Consume_For_The_Same_Message_With_Nack_And_Dead_Letter_Queue();
			//new PMCG.Messaging.Client.AT.Consume.Tests().Publish_1000_Messages_And_Consume_For_The_Same_Messages_With_Ack();
			//new PMCG.Messaging.Client.AT.Consume.Tests().Publish_1000_Messages_And_Consume_For_The_Same_Messages_With_Nack();
			//new PMCG.Messaging.Client.AT.Consume.Tests().Publish_1000_Messages_And_Consume_For_The_Same_Messages_With_Half_Acked_Half_Nacked();
			//new PMCG.Messaging.Client.AT.Consume.Tests().Publish_10000_Messages_And_Consume_For_The_Same_Messages_With_Ack_Blocked_Then_Unblocked();
			//new PMCG.Messaging.Client.AT.Consume.Tests().Publish_10000_Messages_And_Consume_For_The_Same_Messages_With_Ack_Connection_Closed_By_Server_Recovers_Automatically();
		}
	}
}
