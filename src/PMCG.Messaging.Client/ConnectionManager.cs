using log4net;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;


namespace PMCG.Messaging.Client
{
	public class ConnectionManager : IConnectionManager
	{
		private readonly ILog c_logger;
		private readonly Configuration.ConnectionSettings c_connectionSettings;
		private readonly string c_connectionClientProvidedName;
		private readonly TimeSpan c_reconnectionPauseInterval;


		private IConnection c_connection;
		private bool c_isCloseRequested;


		public bool IsOpen { get { return this.c_connection != null && this.c_connection.IsOpen; } }
		public IConnection Connection { get { return this.c_connection; } }


		public ConnectionManager(
			Configuration.ConnectionSettings connectionSettings,
			string connectionClientProvidedName,
			TimeSpan reconnectionPauseInterval)
		{
			this.c_logger = LogManager.GetLogger(this.GetType());
			this.c_logger.Info("ctor Starting");
			
			Check.RequireArgumentNotNull("connectionSettings", connectionSettings);
			Check.RequireArgumentNotEmpty("connectionClientProvidedName", connectionClientProvidedName);
			Check.RequireArgument("reconnectionPauseInterval", reconnectionPauseInterval, reconnectionPauseInterval.Ticks > 0);

			this.c_connectionSettings = connectionSettings;
			this.c_connectionClientProvidedName = connectionClientProvidedName;
			this.c_reconnectionPauseInterval = reconnectionPauseInterval;

			this.c_logger.Info("ctor Completed");
		}


		public void Open(
			uint numberOfTimesToTry = 0)
		{
			this.c_logger.Info("Open Starting");
			Check.Ensure(!this.IsOpen, "Connection is already open");

			var _attemptSequence = 1;
			this.c_isCloseRequested = false;
			while (!this.c_isCloseRequested)
			{
				this.c_logger.InfoFormat("Open Trying to connect, sequence {0}", _attemptSequence);

				var _connectionFactory = new ConnectionFactory
				{
					AutomaticRecoveryEnabled = true,
					Password = this.c_connectionSettings.Password,
					Port = this.c_connectionSettings.Port,
					TopologyRecoveryEnabled = true,
					UseBackgroundThreadsForIO = false,
					UserName = this.c_connectionSettings.UserName,
					VirtualHost = this.c_connectionSettings.VirtualHost
				};

				var _connectionInfo = string.Format("Hosts {0}, port {1}, vhost {2}", string.Join("|", this.c_connectionSettings.HostNames), _connectionFactory.Port, _connectionFactory.VirtualHost);
				this.c_logger.InfoFormat("Open Attempting to connect to ({0}), sequence {1}", _connectionInfo, _attemptSequence);

				try
				{
					this.c_connection = _connectionFactory.CreateConnection(
						this.c_connectionSettings.HostNames,
						this.c_connectionClientProvidedName);

					this.c_logger.InfoFormat("Open Connected to ({0}), sequence {1}", _connectionInfo, _attemptSequence);
				}
				catch (SocketException exception)
				{
					this.c_logger.WarnFormat("Open Failed to connect {0} - {1} {2}", _attemptSequence, exception.GetType().Name, exception.Message);
				}
				catch (BrokerUnreachableException exception)
				{
					this.c_logger.WarnFormat("Open Failed to connect, sequence {0} - {1} {2}", _attemptSequence, exception.GetType().Name, exception.Message);
				}
				catch
				{
					throw;
				}

				if (this.IsOpen) { break; }

				if (this.c_isCloseRequested) { return; }
				if (numberOfTimesToTry > 0 && _attemptSequence == numberOfTimesToTry) { return; }

				Thread.Sleep(this.c_reconnectionPauseInterval);
				_attemptSequence++;
			}

			this.c_logger.Info("Open Completed");
		}


		public void Close()
		{
			this.c_logger.Info("Close Starting");

			this.c_isCloseRequested = true;
			if (this.IsOpen) { this.c_connection.Close(); }

			this.c_logger.Info("Close Completed");
		}
	}
}
