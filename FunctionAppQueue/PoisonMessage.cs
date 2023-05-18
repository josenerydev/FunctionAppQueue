namespace FunctionAppQueue
{
    public class PoisonMessage
    {
        public WeatherForecast OriginalMessage { get; set; }
        public WeatherForecastMetadata Metadata { get; set; }
    }
}