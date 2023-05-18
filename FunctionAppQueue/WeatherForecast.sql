CREATE TABLE WeatherForecast
(
    Id bigint IDENTITY(1,1) PRIMARY KEY,
    Date datetime NOT NULL,
    TemperatureC int NOT NULL,
    Summary nvarchar(100)
);
