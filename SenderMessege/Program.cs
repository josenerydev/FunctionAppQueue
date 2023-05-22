using Azure.Storage.Queues;

using Microsoft.Extensions.ObjectPool;

using SenderMessege;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddSingleton<IQueueClientFactory, QueueClientFactory>();
builder.Services.AddSingleton<IQueueService, QueueService>();

// Add a pool of QueueClient objects
builder.Services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();

builder.Services.AddSingleton<IDictionary<string, ObjectPool<QueueClient>>>(s => new Dictionary<string, ObjectPool<QueueClient>>
{
    { "sample-queue", s.GetRequiredService<ObjectPoolProvider>().Create(new QueueClientPooledObjectPolicy(builder.Configuration, "sample-queue")) },
    { "sample-queue-zombie", s.GetRequiredService<ObjectPoolProvider>().Create(new QueueClientPooledObjectPolicy(builder.Configuration, "sample-queue-zombie")) }
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();