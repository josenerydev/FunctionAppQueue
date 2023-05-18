using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

using Dapper;

using Microsoft.Extensions.Configuration;

namespace FunctionAppQueue
{
    public class Repository : IRepository
    {
        private readonly string _connectionString;

        public Repository(IConfiguration configuration)
        {
            _connectionString = configuration["ConnectionStrings:DefaultConnection"];
        }

        public async Task<WeatherForecast> PostWeatherForecast(WeatherForecast forecast)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string sqlInsert = "INSERT INTO WeatherForecast (Date, TemperatureC, Summary) " +
                                   "OUTPUT Inserted.Id, Inserted.Date, Inserted.TemperatureC, Inserted.Summary " +
                                   "VALUES (@Date, @TemperatureC, @Summary)";

                var insertedForecast = await connection.QueryFirstAsync<WeatherForecast>(sqlInsert, forecast);

                return insertedForecast;
            }
        }

        public async Task<WeatherForecast> GetWeatherForecast(DateTime date)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string sqlSelect = "SELECT Date, TemperatureC, Summary FROM WeatherForecast WHERE Date = @Date";

                var forecast = await connection.QueryFirstOrDefaultAsync<WeatherForecast>(sqlSelect, new { Date = date });

                return forecast;
            }
        }
    }
}