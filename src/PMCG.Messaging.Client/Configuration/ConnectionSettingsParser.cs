using System;
using System.Collections.Generic;
using System.Linq;


namespace PMCG.Messaging.Client.Configuration
{
	public class ConnectionSettingsParser
	{
		public ConnectionSettings Parse(
			string connectionStringSettings)
		{
			Check.RequireArgumentNotEmpty("connectionStringSettings", connectionStringSettings);

			var _settings = connectionStringSettings.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			var _hostNames = this.GetSetting(_settings, "hosts", "localhost").Split(',').ToList();
			var _port = int.Parse(this.GetSetting(_settings, "port", "5672"));
			var _virtualHost = this.GetSetting(_settings, "virtualhost", "/");
			var _clientProvidedName = this.GetSetting(_settings, "clientprovidedname", "clientProvidedName");
			var _userName = this.GetSetting(_settings, "username", "guest");
			var _password = this.GetSetting(_settings, "password", "guest");

			return new ConnectionSettings(_hostNames, _port, _virtualHost, _clientProvidedName, _userName, _password);
		}


		private string GetSetting(
			IEnumerable<string> settings,
			string key,
			string defaultValue)
		{
			var _keyPrefix = string.Format("{0}=", key);
			var _setting = settings.FirstOrDefault(setting => setting.StartsWith(_keyPrefix));
			return _setting != null ? _setting.Substring(_keyPrefix.Length) : defaultValue;
		}
	}
}