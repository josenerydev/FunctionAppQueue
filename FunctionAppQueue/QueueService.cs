using Azure.Storage.Queues;

using FunctionAppQueue;

using Microsoft.Extensions.ObjectPool;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class QueueService : IQueueService
{
    private readonly IDictionary<string, ObjectPool<QueueClient>> _queuePools;

    public QueueService(IDictionary<string, ObjectPool<QueueClient>> queuePools)
    {
        _queuePools = queuePools;
    }

    public async Task SendToPoisonQueueAsync(string message)
    {
        const string queueName = "sample-queue-poison";
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
    }
}