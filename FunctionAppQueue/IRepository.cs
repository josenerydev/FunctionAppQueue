using System;
using System.Threading.Tasks;

namespace FunctionAppQueue
{
    public interface IRepository
    {
        Task<WeatherForecast> GetWeatherForecast(DateTime date);
        Task<WeatherForecast> PostWeatherForecast(WeatherForecast forecast);
    }
}