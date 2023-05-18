namespace FunctionAppQueue
{
    public class PoisonMessage
    {
        public WeatherForecastMessage OriginalMessage { get; set; }
        public WeatherForecastMessageMetadata Metadata { get; set; }
    }
}