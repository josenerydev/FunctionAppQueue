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

                // Create an instance of Random
                var random = new Random();

                // Generate a random number between 0 and 100
                int randomNumber = random.Next(0, 100);

                string sqlInsert;

                // 40% chance to add a delay
                if (randomNumber < 40)
                {
                    sqlInsert = "WAITFOR DELAY '00:01:00'; " +
                                "INSERT INTO WeatherForecast (Date, TemperatureC, Summary) " +
                                "OUTPUT Inserted.Id, Inserted.Date, Inserted.TemperatureC, Inserted.Summary " +
                                "VALUES (@Date, @TemperatureC, @Summary)";
                }
                else
                {
                    sqlInsert = "INSERT INTO WeatherForecast (Date, TemperatureC, Summary) " +
                                "OUTPUT Inserted.Id, Inserted.Date, Inserted.TemperatureC, Inserted.Summary " +
                                "VALUES (@Date, @TemperatureC, @Summary)";
                }

                var insertedForecast = await connection.QueryFirstAsync<WeatherForecast>(sqlInsert, forecast, commandTimeout: 30);

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