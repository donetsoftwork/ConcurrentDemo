using Hand.Tasks;
using Hand.Tasks.Internal;
using Weather.Models;
using Weather.Services;

namespace Concurrent;

/// <summary>
/// 异步结果
/// </summary>
/// <param name="original"></param>
public class WeatherWarpper(int year, int month, int day, DateTime expireTime, CancellationToken token)
    : TaskCallBack<WeatherForecast?>, ITaskJobResult<WeatherForecast?>
{
    #region 配置
    private readonly int _year = year;
    private readonly int _month = month;
    private readonly int _day = day;
    private readonly DateTime _expireTime = expireTime;
    private readonly CancellationToken _token = token;

    /// <inheritdoc />
    public bool Status
        => !_token.IsCancellationRequested && _expireTime >= DateTime.Now;    
    #endregion
    /// <summary>
    /// 获取天气
    /// </summary>
    /// <param name="service"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task GetWeather(IWeatherService service, CancellationToken token)
    {
        if (token.IsCancellationRequested || _token.IsCancellationRequested || _expireTime < DateTime.Now)
        {
            OnCancel();
            return;
        }
        var linked = CancellationTokenSource.CreateLinkedTokenSource(_token, token);
        var weather = await service.GetWeather(_year, _month, _day, linked.Token);
        OnSuccess(weather);
    }
}
