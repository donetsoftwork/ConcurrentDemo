using Weather.Models;

namespace Weather.Services;

public interface IWeatherService
{
    /// <summary>
    /// 获取天气
    /// </summary>
    /// <param name="date"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<WeatherForecast?> GetWeather(int year, int month, int day, CancellationToken token = default);
    /// <summary>
    /// 生成天气数据
    /// </summary>
    /// <param name="year"></param>
    /// <param name="month"></param>
    /// <param name="day"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<WeatherForecast> CreateWeather(int year, int month, int day, CancellationToken token = default);
}
