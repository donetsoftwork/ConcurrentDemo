using Dapper;
using System.Data.Common;
using Weather.Models;

namespace Weather.Services;

public class WeatherService(DbDataSource dataSource)
    : IWeatherService
{
    private readonly DbDataSource _dataSource = dataSource;
    /// <inheritdoc />
    public async Task<WeatherForecast?> GetWeather(int year, int month, int day, CancellationToken token = default)
    {
        var date = new DateTime(year, month, day);
        using var connection = await _dataSource.OpenConnectionAsync(token);
        return await GetWeather(connection, date, token);
    }
    /// <inheritdoc />
    public Task<WeatherForecast?> GetWeather(DbConnection connection, DateTime date, CancellationToken token = default)
    {
        const string sql = """
                SELECT `Date`, `TemperatureC`, `Summary`
                FROM `weathers`
                WHERE `Date`=@date
                """;
        var command = new CommandDefinition(sql, parameters: new { date }, cancellationToken: token);
        return connection.QueryFirstOrDefaultAsync<WeatherForecast>(command);
    }
    /// <summary>
    /// 生成天气数据
    /// </summary>
    /// <param name="year"></param>
    /// <param name="month"></param>
    /// <param name="day"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<WeatherForecast> CreateWeather(int year, int month, int day, CancellationToken token = default)
    {
        var date = new DateTime(year, month, day);
        // 随机生成数据并入库
        var weather = CreateRandomWeather(date);
        await SaveWeather(weather, token);
        return weather;
    }
    /// <summary>
    /// 保存天气
    /// </summary>
    /// <param name="weather"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task SaveWeather(WeatherForecast weather, CancellationToken token = default)
    {
        const string sql = """
                INSERT INTO `weathers`(`Date`, `TemperatureC`, `Summary`)
                VALUES(@Date, @TemperatureC, @Summary)
                """;
        var command = new CommandDefinition(sql, parameters: weather, cancellationToken: token);
        using var connection = await _dataSource.OpenConnectionAsync(token);
        await connection.ExecuteAsync(command);
    }
    #region CreateRandomWeather
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];
    /// <summary>
    /// 随机生成天气
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public static WeatherForecast CreateRandomWeather(DateTime date)
    {
        return new WeatherForecast
        {
            Date = date,
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        };
    }
    #endregion
}
