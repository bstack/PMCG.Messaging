using System;
using System.Collections.Generic;


namespace PMCG.Messaging.Client.Configuration
{
	public class ConnectionSettings
	{
		public readonly IList<string> HostNames;
		public readonly int Port;
		public readonly string VirtualHost;
		public readonly string ClientProvidedName;
		public readonly string UserName;
		public readonly string Password;


		public ConnectionSettings(
			IList<string> hostNames,
			int port,
			string virtualHost,
			string clientProvidedName,
			string userName,
			string password)
		{
			Check.RequireArgumentNotNull("hostNames", hostNames);
			Check.RequireArgument("port", port, port > 0);
			Check.RequireArgumentNotEmpty("virtualHost", virtualHost);
			Check.RequireArgumentNotEmpty("clientProvidedName", clientProvidedName);
			Check.RequireArgumentNotEmpty("userName", userName);
			Check.RequireArgumentNotEmpty("password", password);

			this.HostNames = hostNames;
			this.Port = port;
			this.VirtualHost = virtualHost;
			this.ClientProvidedName = clientProvidedName;
			this.UserName = userName;
			this.Password = password;
		}
	}
}