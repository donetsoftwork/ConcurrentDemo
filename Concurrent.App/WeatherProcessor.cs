using Hand.Collections;
using Hand.Job;
using Hand.States;
using Weather.Models;
using Weather.Services;

namespace Concurrent;

/// <summary>
/// 天气处理器
/// </summary>
/// <param name="queue"></param>
public sealed class WeatherProcessor(IQueue<WeatherWarpper> queue, IWeatherService originalService, TimeSpan expireTime)
    : IQueueProcessor<WeatherWarpper>
    , IWeatherService
{
    #region 配置
    /// <summary>
    /// 队列
    /// </summary>
    private readonly IQueue<WeatherWarpper> _queue = queue;
    /// <summary>
    /// 原始服务
    /// </summary>
    private readonly IWeatherService _originalService = originalService;
    /// <summary>
    /// 过期事件
    /// </summary>
    private readonly TimeSpan _expireTime = expireTime;
    /// <summary>
    /// 队列
    /// </summary>
    public IQueue<WeatherWarpper> Queue
        => _queue;
    #endregion
    #region IWeatherService
    /// <inheritdoc />
    public Task<WeatherForecast?> GetWeather(int year, int month, int day, CancellationToken token)
    {
        var result = new WeatherWarpper(year, month, day, DateTime.Now.Add(_expireTime), token);
        if (token.IsCancellationRequested)
        {
            Cancel(result);     
        }
        else
        {
            _queue.Enqueue(result);
        }
        return result.Task;
    }
    /// <inheritdoc />
    Task<WeatherForecast> IWeatherService.CreateWeather(int year, int month, int day, CancellationToken token)
        => _originalService.CreateWeather(year, month, day, token);
    #endregion
    #region IQueueProcessor<WeatherResult>
    /// <inheritdoc />
    async void IQueueProcessor<WeatherWarpper>.Run(IQueue<WeatherWarpper> queue, ThreadJobService<WeatherWarpper> service, CancellationToken token)
    {
        while (queue.TryDequeue(out var item))
        {
            if (service.Activate(item))
            {
                try
                {
                    await item.GetWeather(_originalService, token);
                }
                catch (Exception ex)
                {
                    Exception(item, ex);
                }
            }
            else
            {
                Cancel(item);
                break;
            }
            if (token.IsCancellationRequested)
                break;
        }
        // 线程用完释放(回收)
        service.Dispose();
    }
    #endregion
    /// <summary>
    /// 异常处理
    /// </summary>
    /// <param name="callBack"></param>
    /// <param name="ex"></param>

    public static void Exception(IExceptionable callBack, Exception ex)
    {
        try
        {
            callBack.OnException(ex);
        }
        catch { }
    }
    /// <summary>
    /// 取消
    /// </summary>
    /// <param name="cancelable"></param>
    public static void Cancel(ICancelable cancelable)
    {
        try
        {
            cancelable.OnCancel();
        }
        catch { }
    }
}
