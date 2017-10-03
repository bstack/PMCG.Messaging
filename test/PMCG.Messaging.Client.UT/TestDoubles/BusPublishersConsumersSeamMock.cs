using PMCG.Messaging.Client.Configuration;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;


namespace PMCG.Messaging.Client.UT.TestDoubles
{
    public class BusPublishersConsumersSeamMock : PMCG.Messaging.Client.IBusPublishersConsumersSeam
    {
        private readonly PublicationResultStatus c_publicationResultStatus;


        public BusPublishersConsumersSeamMock(
            PublicationResultStatus publicationResultStatus)
        {
            this.c_publicationResultStatus = publicationResultStatus;
        }


        public void CreateConsumers(
            BusConfiguration configuration,
            IConnectionManager connectionManager,
            BlockingCollection<Publication> publicationQueue)
        {
            //throw new NotImplementedException();
        }

        public void CreatePublishers(
            BusConfiguration configuration,
            IConnectionManager connectionManager,
            BlockingCollection<Publication> publicationQueue)
        {
            var _task = new Task(() =>
            {
                foreach (var _publication in publicationQueue.GetConsumingEnumerable())
                {
                    _publication.SetResult(this.c_publicationResultStatus);
                }
            });
            _task.Start();
        }
    }
}
