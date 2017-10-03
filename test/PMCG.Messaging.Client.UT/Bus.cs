using NSubstitute;
using NUnit.Framework;
using PMCG.Messaging.Client.Configuration;
using PMCG.Messaging.Client.UT.TestDoubles;
using System;
using System.Threading.Tasks;


namespace PMCG.Messaging.Client.UT
{
	[TestFixture]
	public class Bus
	{
        private BusConfiguration c_busConfiguration;
        private IConnectionManager c_connectionManager;


        [SetUp]
        public void SetUp()
        {
            var _busConfigurationBuilder = new BusConfigurationBuilder();
            _busConfigurationBuilder.ConnectionUris.Add(TestingConfiguration.LocalConnectionUri);
            _busConfigurationBuilder.ConnectionClientProvidedName = TestingConfiguration.ConnectionClientProvidedName;
            _busConfigurationBuilder.RegisterPublication<MyEvent>("", typeof(MyEvent).Name, MessageDeliveryMode.Persistent, message => "test.queue.1");
            this.c_busConfiguration = _busConfigurationBuilder.Build();
            this.c_connectionManager = Substitute.For<IConnectionManager>();
            this.c_connectionManager.IsOpen.ReturnsForAnyArgs(true);
        }


        [Test]
        public void Ctor_Valid()
        {
            var _result = new PMCG.Messaging.Client.Bus(this.c_busConfiguration);

            Assert.IsNotNull(_result);
        }


        // NOTE: Unusual to expected an exception of NullReferenceException, see ctor note
        [Test]
        public void Ctor_Invalid_Null_BusConfiguration_Exception()
        {
            Assert.That(() => new PMCG.Messaging.Client.Bus(null), Throws.TypeOf<NullReferenceException>());
        }


        [Test]
        public void Connect_Invalid_ConnectionManager_Connection_Is_Closed_Exception()
        { 
            var _busPublishersConsumersSeam = Substitute.For<IBusPublishersConsumersSeam>();
            var _connectionManager = Substitute.For<IConnectionManager>();
            _connectionManager.IsOpen.ReturnsForAnyArgs(false);
            var _SUT = new PMCG.Messaging.Client.Bus(this.c_busConfiguration, _busPublishersConsumersSeam ,_connectionManager);

            Assert.That(() => _SUT.Connect(), Throws.TypeOf<ApplicationException>());
        }


        [Test]
        public void Connect_Valid()
        {
            var _busPublishersConsumersSeam = Substitute.For<IBusPublishersConsumersSeam>();
            var _SUT = new PMCG.Messaging.Client.Bus(this.c_busConfiguration, _busPublishersConsumersSeam, this.c_connectionManager);

            _SUT.Connect();
        }


        [Test]
        public void PublishAsync_Null_Message_Results_In_An_Exception()
        {
            var _busPublishersConsumersSeam = new BusPublishersConsumersSeamMock(PublicationResultStatus.Nacked);
            var _SUT = new PMCG.Messaging.Client.Bus(this.c_busConfiguration, _busPublishersConsumersSeam, this.c_connectionManager);
            _SUT.Connect();

            Assert.That(() => _SUT.PublishAsync<MyEvent>(null), Throws.TypeOf<ArgumentNullException>());
        }


        [Test]
        public void PublishAsync_Valid_Does_Publication_Configuration_Exists()
        {
            var _busConfigurationBuilder = new BusConfigurationBuilder();
            _busConfigurationBuilder.ConnectionUris.Add(TestingConfiguration.LocalConnectionUri);
            _busConfigurationBuilder.ConnectionClientProvidedName = TestingConfiguration.ConnectionClientProvidedName;
            var _busConfiguration = _busConfigurationBuilder.Build();
            var _busPublishersConsumersSeam = new BusPublishersConsumersSeamMock(PublicationResultStatus.None);
            var _connectionManager = Substitute.For<IConnectionManager>();
            _connectionManager.IsOpen.ReturnsForAnyArgs(true);
            var _SUT = new PMCG.Messaging.Client.Bus(_busConfiguration, _busPublishersConsumersSeam, _connectionManager);
            _SUT.Connect();
            var _message = new MyEvent(Guid.NewGuid(), "correlationid1", "detail", 1);

            var _result = _SUT.PublishAsync(_message);
            _result.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, _result.Status);
            Assert.AreEqual(PMCG.Messaging.PublicationResultStatus.NoConfigurationFound, _result.Result.Status);
        }


        [Test]
        public void PublishAsync_Valid_Acked()
        {
            var _busPublishersConsumersSeam = new BusPublishersConsumersSeamMock(PublicationResultStatus.Acked);
            var _SUT = new PMCG.Messaging.Client.Bus(this.c_busConfiguration, _busPublishersConsumersSeam, this.c_connectionManager);
            _SUT.Connect();
            var _message = new MyEvent(Guid.NewGuid(), "correlationid1", "detail", 1);

            var _result = _SUT.PublishAsync(_message);
            _result.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, _result.Status);
            Assert.AreEqual(PMCG.Messaging.PublicationResultStatus.Published, _result.Result.Status);
        }


        [Test]
        public void PublishAsync_Valid_Nacked()
        {
            var _busPublishersConsumersSeam = new BusPublishersConsumersSeamMock(PublicationResultStatus.Nacked);
            var _SUT = new PMCG.Messaging.Client.Bus(this.c_busConfiguration, _busPublishersConsumersSeam, this.c_connectionManager);
            _SUT.Connect();
            var _message = new MyEvent(Guid.NewGuid(), "correlationid1", "detail", 1);

            var _result = _SUT.PublishAsync(_message);
            _result.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, _result.Status);
            Assert.AreEqual(PMCG.Messaging.PublicationResultStatus.NotPublished, _result.Result.Status);
        }


        [Test]
        public void PublishAsync_Valid_Channel_Shutdown()
        {
            var _busPublishersConsumersSeam = new BusPublishersConsumersSeamMock(PublicationResultStatus.ChannelShutdown);
            var _SUT = new PMCG.Messaging.Client.Bus(this.c_busConfiguration, _busPublishersConsumersSeam, this.c_connectionManager);
            _SUT.Connect();
            var _message = new MyEvent(Guid.NewGuid(), "correlationid1", "detail", 1);

            var _result = _SUT.PublishAsync(_message);
            _result.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, _result.Status);
            Assert.AreEqual(PMCG.Messaging.PublicationResultStatus.NotPublished, _result.Result.Status);
        }


        [Test]
        public void PublishAsync_Where_Multiple_Publication_Configurations_Valid_Acked()
        {
            var _busConfigurationBuilder = new BusConfigurationBuilder();
            _busConfigurationBuilder.ConnectionUris.Add(TestingConfiguration.LocalConnectionUri);
            _busConfigurationBuilder.ConnectionClientProvidedName = TestingConfiguration.ConnectionClientProvidedName;
            _busConfigurationBuilder.RegisterPublication<MyEvent>("", typeof(MyEvent).Name, MessageDeliveryMode.Persistent, message => "test.queue.1");
            _busConfigurationBuilder.RegisterPublication<MyEvent>("", typeof(MyEvent).Name, MessageDeliveryMode.Persistent, message => "test.queue.1");
            var _busConfiguration = _busConfigurationBuilder.Build();
            var _busPublishersConsumersSeam = new BusPublishersConsumersSeamMock(PublicationResultStatus.Acked);
            var _SUT = new PMCG.Messaging.Client.Bus(_busConfiguration, _busPublishersConsumersSeam, this.c_connectionManager);
            _SUT.Connect();
            var _message = new MyEvent(Guid.NewGuid(), "correlationid1", "detail", 1);

            var _result = _SUT.PublishAsync(_message);
            _result.Wait();

            Assert.AreEqual(TaskStatus.RanToCompletion, _result.Status);
            Assert.AreEqual(PMCG.Messaging.PublicationResultStatus.Published, _result.Result.Status);
        }
    }
}
