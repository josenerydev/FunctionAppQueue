using Azure.Storage.Queues;

using Microsoft.Extensions.ObjectPool;

using SenderMessege;

public class QueueService : IQueueService
{
    private readonly IDictionary<string, ObjectPool<QueueClient>> _queuePools;

    public QueueService(IDictionary<string, ObjectPool<QueueClient>> queuePools)
    {
        _queuePools = queuePools;
    }

    public async Task SendMessageAsync(string message, string queueName)
    {
        var queuePool = _queuePools[queueName];
        var queueClient = queuePool.Get();
        TimeSpan neverExpireMessageTimeSpan = TimeSpan.FromSeconds(-1);

        try
        {
            await queueClient.CreateIfNotExistsAsync();

            await queueClient.SendMessageAsync(message, timeToLive: neverExpireMessageTimeSpan);
        }
        finally
        {
            queuePool.Return(queueClient);
        }

        // Send the same message to the zombie queue
        var zombieQueueName = $"{queueName}-zombie";

        var zombieQueuePool = _queuePools[zombieQueueName];
        var zombieQueueClient = zombieQueuePool.Get();

        try
        {
            await zombieQueueClient.CreateIfNotExistsAsync();
            await zombieQueueClient.SendMessageAsync(message, timeToLive: neverExpireMessageTimeSpan);
        }
        finally
        {
            zombieQueuePool.Return(zombieQueueClient);
        }
    }
}