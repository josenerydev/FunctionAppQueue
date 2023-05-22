using Azure.Storage.Queues;

namespace SenderMessege
{
    public class QueueClientFactory : IQueueClientFactory
    {
        private readonly string _connectionString;
        private readonly QueueClientOptions _queueClientOptions;

        public QueueClientFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MyStorageConnection");
            _queueClientOptions = new QueueClientOptions { MessageEncoding = QueueMessageEncoding.Base64 };
        }

        public QueueClient Create(string queueName)
        {
            return new QueueClient(_connectionString, queueName, _queueClientOptions);
        }
    }
}