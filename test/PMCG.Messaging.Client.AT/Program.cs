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

			var _connectTests = new PMCG.Messaging.Client.AT.Connect.Tests();
			var _publishTests = new PMCG.Messaging.Client.AT.Publish.Tests();
			var _consumeTests = new PMCG.Messaging.Client.AT.Consume.Tests();

			//_connectTests.Connect_Non_Existent_Broker_Indefinitely();
			//_connectTests.Connect_Restart_Broker_Connection_Reestablished_Automatically();
			//_connectTests.Connect_Close_The_Connection_Using_The_Management_UI_Connection_Reestablished_Automatically();
			_connectTests.Connect_Is_Already_Started_Then_Blocked_And_Then_Unblocked();

			//_publishTests.Publish_A_Message_To_A_Queue_Using_The_Direct_Exchange();
			//_publishTests.Publish_A_Message_To_A_Queue_Using_Custom_Exchange();
			//_publishTests.Publish_A_Message_To_Two_Exchanges();
			// TODO Talk about this one, OnChanelShutdown called and no exception thrown??????
			//_publishTests.Publish_A_Message_To_An_Exchange_That_Doesnt_Exist();
			//_publishTests.Publish_1000_Messages_To_A_Queue_Using_Custom_Exchange();
			//_publishTests.Publish_With_Timeout();
			//_publishTests.Publish_Connection_Closed_By_Application_Never_Recovers();
			// TODO Messages disappearing, not expected when running slow??????????
			//_publishTests.Publish_Connection_Closed_By_Server_Recovers_Automatically();
			//_publishTests.Publish_Connection_Closed_By_Server_Restart_Unpublished_Messages_Are_Republished_Successfully();
			//_publishTests.Publish_Connection_Blocked_Then_Unblocked_Unpublished_Messages_Are_Republished_Successfully();
			//_publishTests.Publish_A_Message_That_Expires_Ends_Up_In_Dead_Letter_Queue();

			//_consumeTests.Publish_A_Message_And_Consume_For_The_Same_Message_With_Ack();
			//_consumeTests.Publish_A_Message_And_Consume_For_The_Same_Message_With_Nack();
			//_consumeTests.Publish_A_Message_And_Consume_For_The_Same_Message_With_Nack_And_Dead_Letter_Queue();
			//_consumeTests.Publish_1000_Messages_And_Consume_For_The_Same_Messages_With_Ack();
			//_consumeTests.Publish_1000_Messages_And_Consume_For_The_Same_Messages_With_Nack();
			//_consumeTests.Publish_1000_Messages_And_Consume_For_The_Same_Messages_With_Half_Acked_Half_Nacked();
			//_consumeTests.Publish_10000_Messages_And_Consume_For_The_Same_Messages_With_Ack_Blocked_Then_Unblocked();
			//_consumeTests.Publish_10000_Messages_And_Consume_For_The_Same_Messages_With_Ack_Connection_Closed_By_Server_Recovers_Automatically();
			//_consumeTests.Publish_10000_Messages_And_Consume_On_Separate_Bus_For_The_Same_Messages_Consumer_Connection_Closed_By_Server_Recovers_Automatically();
			//_consumeTests.Publish_100_Messages_And_Consume_For_The_Same_Messsage_On_A_Transient_Queue();
		}
	}
}
