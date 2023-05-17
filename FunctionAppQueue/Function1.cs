using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

using System.Text.Json;

namespace FunctionAppQueue
{
    public class Function1
    {
        [FunctionName("Function1")]
        public void Run([QueueTrigger("sample-queue")] string myQueueItem, ILogger log)
        {
            WeatherForecast forecast = JsonSerializer.Deserialize<WeatherForecast>(myQueueItem);

            log.LogWarning($"C# Queue trigger function processed: {forecast.Date}, {forecast.TemperatureC}, {forecast.Summary}");
        }
    }
}