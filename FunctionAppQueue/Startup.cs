using Azure.Storage.Queues;

using FunctionAppQueue;

using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;

using System.Collections.Generic;

[assembly: FunctionsStartup(typeof(Startup))]

namespace FunctionAppQueue
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = builder.GetContext().Configuration;
            builder.Services.AddSingleton<IRepository, Repository>();
            builder.Services.AddSingleton<IQueueService, QueueService>();

            // Add a pool of QueueClient objects
            builder.Services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();

            builder.Services.AddSingleton<IDictionary<string, ObjectPool<QueueClient>>>(s => new Dictionary<string, ObjectPool<QueueClient>>
            {
                { "sample-queue-poison", s.GetRequiredService<ObjectPoolProvider>().Create(new QueueClientPooledObjectPolicy(configuration, "sample-queue-poison")) }
            });
        }
    }
}