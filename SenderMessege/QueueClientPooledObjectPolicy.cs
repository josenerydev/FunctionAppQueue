using Azure.Storage.Queues;

using Microsoft.Extensions.ObjectPool;

namespace SenderMessege
{
    public class QueueClientPooledObjectPolicy : PooledObjectPolicy<QueueClient>
    {
        private readonly string _connectionString;
        private readonly string _queueName;

        public QueueClientPooledObjectPolicy(IConfiguration configuration, string queueName)
        {
            _connectionString = configuration.GetConnectionString("MyStorageConnection");
            _queueName = queueName;
        }

        public override QueueClient Create()
        {
            return new QueueClient(_connectionString, _queueName, new QueueClientOptions { MessageEncoding = QueueMessageEncoding.Base64 });
        }

        public override bool Return(QueueClient obj)
        {
            // You could add additional logic here to handle
            // objects that might be broken or expired.
            return true;
        }
    }
}