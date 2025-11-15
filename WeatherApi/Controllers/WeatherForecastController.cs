using Microsoft.AspNetCore.Mvc;
using Weather.Models;
using Weather.Services;

namespace WeatherApi.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController(IWeatherService service, ILogger<WeatherForecastController> logger)
    : ControllerBase
{
    private readonly IWeatherService _service = service;
    private readonly ILogger<WeatherForecastController> _logger = logger;

    /// <summary>
    /// 查看天气
    /// </summary>
    /// <param name="year"></param>
    /// <param name="month"></param>
    /// <param name="day"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    [HttpGet("{year}/{month}/{day}")]
    [ProducesResponseType<WeatherForecast>(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType<Exception>(500)]
    public async Task<IActionResult> Get(int year, int month, int day, CancellationToken token = default)
    {
        WeatherForecast? weather;
        try
        {
            weather = await _service.GetWeather(year, month, day, token);
        }
        catch (Exception ex)
        {

            _logger.LogError(ex, "An error occurred while getting weather for {Year}/{Month}/{Day}", year, month, day);
            return BadRequest(ex.Message);
        }
        _logger.LogInformation("Get /{Year}/{Month}/{Day}", year, month, day);
        if (weather is null)
            return NotFound();
        return Ok(weather);
    }
    /// <summary>
    /// 生成天气数据
    /// </summary>
    /// <param name="year"></param>
    /// <param name="month"></param>
    /// <param name="day"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    [HttpPut("{year}/{month}/{day}")]
    [ProducesResponseType<WeatherForecast>(200)]
    [ProducesResponseType<Exception>(500)]
    public async Task<IActionResult> Put(int year, int month, int day, CancellationToken token = default)
    {
        WeatherForecast weather;
        try
        {
            weather = await _service.CreateWeather(year, month, day, token);
        }
        catch (Exception ex)
        {

            _logger.LogError(ex, "An error occurred while getting weather for {Year}/{Month}/{Day}", year, month, day);
            return BadRequest(ex.Message);
        }
        _logger.LogInformation("Put /{Year}/{Month}/{Day}", year, month, day);
        return Ok(weather);
    }
}
