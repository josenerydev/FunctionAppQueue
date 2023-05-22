using Azure.Storage.Queues;

namespace SenderMessege
{
    public interface IQueueClientFactory
    {
        QueueClient Create(string queueName);
    }
}