using NUnit.Framework;
using PMCG.Messaging.Client.Configuration;
using System;
using System.Linq;


namespace PMCG.Messaging.Client.UT.Configuration
{
	[TestFixture]
	public class ConnectionSettingsParserSpec
	{
		private ConnectionSettingsParser c_SUT = new ConnectionSettingsParser();


		[Test]
		public void Parse_Where_Null_String_Results_In_An_Exception()
		{
			Assert.That(() => this.c_SUT.Parse(null), Throws.TypeOf<ArgumentException>());
		}


		[Test]
		public void Parse_Where_Empty_String_Results_In_An_Exception()
		{
			Assert.That(() => this.c_SUT.Parse(" "), Throws.TypeOf<ArgumentException>());
		}


		[Test]
		public void Parse_Where_Single_Host_Results_In_A_Single_ConnectionSettings_Object()
		{
			var _result = this.c_SUT.Parse("hosts=localhost;port=5672;virtualhost=/;username=guest;ispasswordencrypted=false;password=Pass");

			Assert.IsNotNull(_result);
			Assert.AreEqual(1, _result.HostNames.Count());
			Assert.AreEqual("localhost", _result.HostNames.Single());
			Assert.AreEqual(5672, _result.Port);
			Assert.AreEqual("guest", _result.UserName);
			Assert.AreEqual("Pass", _result.Password);
			Assert.AreEqual("/", _result.VirtualHost);
		}


		[Test]
		public void Parse_Where_Multiple_Hosts_Results_In_A_Multiple_Connection_Strings()
		{
			var _result = this.c_SUT.Parse("hosts=host1,host2;port=5672;virtualhost=/;username=guest;password=thepass");

			Assert.IsNotNull(_result);
			Assert.AreEqual(2, _result.HostNames.Count());

			Assert.AreEqual("host1", _result.HostNames.First());
			Assert.AreEqual(5672, _result.Port);
			Assert.AreEqual("guest", _result.UserName);
			Assert.AreEqual("thepass", _result.Password);
			Assert.AreEqual("/", _result.VirtualHost);

			Assert.AreEqual("host2", _result.HostNames.Last());
			Assert.AreEqual(5672, _result.Port);
			Assert.AreEqual("guest", _result.UserName);
			Assert.AreEqual("thepass", _result.Password);
			Assert.AreEqual("/", _result.VirtualHost);
		}


		[Test]
		public void Parse_Where_Single_Host_With_Encrypted_Password_Results_In_A_Single_Connection_String()
		{
			var _passwordCipher = new DefaultPasswordParser().Encrypt("ThePassword");
			var _connectionStringSettings = string.Format("hosts=localhost;port=5672;virtualhost=/;username=guest;ispasswordencrypted=true;password={0}:{1}", Environment.MachineName, _passwordCipher);

			var _result = this.c_SUT.Parse(_connectionStringSettings);

			Assert.IsNotNull(_result);
			Assert.AreEqual(1, _result.HostNames.Count());
			Assert.AreEqual("localhost", _result.HostNames.Single());
			Assert.AreEqual(5672, _result.Port);
			Assert.AreEqual("guest", _result.UserName);
			Assert.AreEqual("ThePassword", _result.Password);
			Assert.AreEqual("/", _result.VirtualHost);
		}



		[Test]
		public void Parse_Where_Only_Hosts_Specified_Results_In_A_Multiple_Connection_Strings()
		{
			var _result = this.c_SUT.Parse("hosts=host1,host2");

			Assert.AreEqual("host1", _result.HostNames.First());
			Assert.AreEqual(5672, _result.Port);
			Assert.AreEqual("guest", _result.UserName);
			Assert.AreEqual("guest", _result.Password);
			Assert.AreEqual("/", _result.VirtualHost);

			Assert.AreEqual("host2", _result.HostNames.Last());
			Assert.AreEqual(5672, _result.Port);
			Assert.AreEqual("guest", _result.UserName);
			Assert.AreEqual("guest", _result.Password);
			Assert.AreEqual("/", _result.VirtualHost);
		}
	}
}