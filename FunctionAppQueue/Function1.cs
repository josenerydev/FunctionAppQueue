using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

using System.Text.Json;
using System.Threading.Tasks;

namespace FunctionAppQueue
{
    public class Function1
    {
        private readonly IRepository _repository;

        public Function1(IRepository repository)
        {
            _repository = repository;
        }

        [FunctionName("ProcessQueueFunction")]
        public async Task ProcessQueueMessage(
            [QueueTrigger("sample-queue")] string myQueueItem,
            [Queue("sample-queue-poison")] IAsyncCollector<string> poisonQueueCollector,
            ILogger log)
        {
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

                var messageWithMetadata = new PoisonMessage
                {
                    OriginalMessage = forecast.ToWeatherForecastMessage(),
                    Metadata = new WeatherForecastMessageMetadata
                    {
                        Error = "Timeout error",
                        ExceptionMessage = ex.Message
                    }
                };

                var messageJson = JsonSerializer.Serialize(messageWithMetadata);
                await poisonQueueCollector.AddAsync(messageJson);
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                log.LogError($"Failed to save forecast to the database: {ex.Message}");

                var messageWithMetadata = new PoisonMessage
                {
                    OriginalMessage = forecast.ToWeatherForecastMessage(),
                    Metadata = new WeatherForecastMessageMetadata
                    {
                        Error = "Database error",
                        ExceptionMessage = ex.Message
                    }
                };

                var messageJson = JsonSerializer.Serialize(messageWithMetadata);
                await poisonQueueCollector.AddAsync(messageJson);
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
            var message = JsonSerializer.Deserialize<PoisonMessage>(myQueueItem);

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