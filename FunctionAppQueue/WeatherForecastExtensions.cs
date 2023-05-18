namespace FunctionAppQueue
{
    public static class WeatherForecastExtensions
    {
        public static WeatherForecast ToWeatherForecast(this WeatherForecastMessage message)
        {
            return new WeatherForecast
            {
                Date = message.Date,
                TemperatureC = message.TemperatureC,
                Summary = message.Summary
            };
        }

        public static WeatherForecastMessage ToWeatherForecastMessage(this WeatherForecast forecast)
        {
            return new WeatherForecastMessage
            {
                Date = forecast.Date,
                TemperatureC = forecast.TemperatureC,
                Summary = forecast.Summary
            };
        }
    }

}