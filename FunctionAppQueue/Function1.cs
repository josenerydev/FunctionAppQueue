using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace FunctionAppQueue
{
    public class Function1
    {
        private readonly IRepository _repository;
        private readonly IQueueService _queueService;

        public Function1(IRepository repository, IQueueService queueService)
        {
            _repository = repository;
            _queueService = queueService;
        }

        [FunctionName("ProcessQueueFunction")]
        public async Task ProcessQueueMessage(
            [QueueTrigger("sample-queue")] string myQueueItem,
            ILogger log)
        {
            try
            {
                Random rand = new Random();
                int randomNumber = rand.Next(10);

                // 30% chance of throwing an exception
                if (randomNumber < 3)
                {
                    throw new Exception("Random exception occurred");
                }

                WeatherForecast forecast = JsonSerializer.Deserialize<WeatherForecast>(myQueueItem);

                log.LogWarning($"C# Queue trigger function processed: {forecast.Date}, {forecast.TemperatureC}, {forecast.Summary}");

                try
                {
                    // Save the forecast to the database
                    await _repository.PostWeatherForecast(forecast);
                }
                catch (System.Data.SqlClient.SqlException ex) when (ex.Number == -2) // TimeOut Exception
                {
                    log.LogError($"Timeout error while saving forecast to the database: {ex.Message}");

                    var messageWithMetadata = new PoisonMessage<WeatherForecastMessage>
                    {
                        OriginalMessage = forecast.ToWeatherForecastMessage(),
                        Metadata = new MessageMetadata
                        {
                            Error = "Timeout error",
                            ExceptionMessage = ex.Message
                        }
                    };

                    var messageJson = JsonSerializer.Serialize(messageWithMetadata);
                    await _queueService.SendToPoisonQueueAsync(messageJson);
                }
                catch (System.Data.SqlClient.SqlException ex)
                {
                    log.LogError($"Failed to save forecast to the database: {ex.Message}");

                    var messageWithMetadata = new PoisonMessage<WeatherForecastMessage>
                    {
                        OriginalMessage = forecast.ToWeatherForecastMessage(),
                        Metadata = new MessageMetadata
                        {
                            Error = "Database error",
                            ExceptionMessage = ex.Message
                        }
                    };

                    var messageJson = JsonSerializer.Serialize(messageWithMetadata);
                    await _queueService.SendToPoisonQueueAsync(messageJson);
                }
            }
            catch (Exception ex)
            {
                log.LogError($"Fatal error: {ex.Message}");

                var messageWithMetadata = new PoisonMessage<string>
                {
                    OriginalMessage = myQueueItem,
                    Metadata = new MessageMetadata
                    {
                        Error = "Fatal error",
                        ExceptionMessage = ex.Message
                    }
                };

                var messageJson = JsonSerializer.Serialize(messageWithMetadata);
                await _queueService.SendToPoisonQueueAsync(messageJson);
            }
        }

        [FunctionName("PoisonQueueFunction")]
        public static async Task ProcessPoisonQueueMessage(
            [QueueTrigger("sample-queue-poison")] string myQueueItem,
            [Queue("sample-queue")] IAsyncCollector<string> queueCollector,
            ILogger log)
        {
            log.LogError($"Processing poison message: {myQueueItem}");

            // Deserialize the message to access its metadata
            var message = JsonSerializer.Deserialize<PoisonMessage<WeatherForecast>>(myQueueItem);

            // Check if the Error metadata is "Timeout error"
            if (message.Metadata.Error == "Timeout error")
            {
                // Attempt to process the message again by adding it back to the original queue
                var originalMessageJson = JsonSerializer.Serialize(message.OriginalMessage);
                await queueCollector.AddAsync(originalMessageJson);
            }
        }
    }
}