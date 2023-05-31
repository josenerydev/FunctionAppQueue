using System;
using System.Data;
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

        public async Task Test(WeatherForecast forecast)
        {
            var address = new DataTable();
            address.Columns.Add("Street", typeof(string));
            address.Columns.Add("City", typeof(string));
            address.Columns.Add("State", typeof(string));
            address.Columns.Add("ZipCode", typeof(string));

            address.Rows.Add("123 Main St.", "Smallville", "KS", "12345");

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Create an instance of Random
                var parameters = new DynamicParameters();

                parameters.Add("@Addresses", address.AsTableValuedParameter("Address"));

                connection.Execute("YourStoredProcedure", parameters, commandType: CommandType.StoredProcedure);
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