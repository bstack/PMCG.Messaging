using System;
using System.Collections.Generic;


namespace PMCG.Messaging.Client.UT.TestDoubles
{
	public class ConnectionSettingsBuilder
	{
		public PMCG.Messaging.Client.Configuration.ConnectionSettings Build(
			IList<string> hostNames = null,
			int port = 5672,
			string virtualHost = "/",
			string clientProvidedName = "clientProvidedName",
			string userName = "guest",
			string password = "guest")
		{
			if(hostNames == null) { hostNames = new List<string>() { "localhost" }; }

			return new PMCG.Messaging.Client.Configuration.ConnectionSettings(
				hostNames, port, virtualHost, clientProvidedName, userName, password);
		}
	}
}
