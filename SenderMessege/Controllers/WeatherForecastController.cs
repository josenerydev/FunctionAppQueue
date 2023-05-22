using Microsoft.AspNetCore.Mvc;

using System.Text.Json;

namespace SenderMessege.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly IQueueService _queueService;
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IQueueService queueService)
        {
            _logger = logger;
            _queueService = queueService;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            var weatherForecast = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();

            foreach (var forecast in weatherForecast)
            {
                string message = JsonSerializer.Serialize(forecast);

                await _queueService.SendMessageAsync(message, "sample-queue");
            }

            return weatherForecast;
        }
    }
}