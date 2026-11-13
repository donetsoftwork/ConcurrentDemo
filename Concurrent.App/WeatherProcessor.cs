using Hand.Collections;
using Hand.Job;
using Hand.States;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using System.Data;
using System.Data.Common;
using Weather.Models;
using Weather.Services;

namespace Concurrent;

/// <summary>
/// 天气处理器
/// </summary>
/// <param name="queue"></param>
/// <param name="dataSource"></param>
/// <param name="originalService"></param>
/// <param name="expireTime"></param>
public sealed class WeatherProcessor(IQueue<WeatherWarpper> queue, DbDataSource dataSource, IWeatherService originalService, TimeSpan expireTime, ILogger<WeatherProcessor> logger)
    : IQueueProcessor<WeatherWarpper>
    , IWeatherService
{
    #region 配置
    /// <summary>
    /// 队列
    /// </summary>
    private readonly IQueue<WeatherWarpper> _queue = queue;
    private readonly DbDataSource _dataSource = dataSource;

    /// <summary>
    /// 原始服务
    /// </summary>
    private readonly IWeatherService _originalService = originalService;
    /// <summary>
    /// 过期时间
    /// </summary>
    private readonly TimeSpan _expireTime = expireTime;
    /// <summary>
    /// 日志
    /// </summary>
    private readonly ILogger<WeatherProcessor> _logger = logger;

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
        if (token.IsCancellationRequested)
            return Task.FromCanceled<WeatherForecast?>(token);

        var date = new DateTime(year, month, day);
        var result = new WeatherWarpper(date, DateTime.Now.Add(_expireTime), token);
        _queue.Enqueue(result);
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
        //using var connection = new MySqlConnection(_dataSource.ConnectionString);
        //await connection.OpenAsync(CancellationToken.None)
        //    .ConfigureAwait(false);
        //using var connection = await _dataSource.OpenConnectionAsync(CancellationToken.None);
        while (queue.TryDequeue(out var item))
        {
            if (service.Activate(item))
            {
                //bool wasClosed = connection.State == ConnectionState.Closed;
                try
                {
                    //if(wasClosed)
                    //{
                    //    await connection.OpenAsync(CancellationToken.None)
                    //        .ConfigureAwait(false);
                    //}
                    using var connection = await _dataSource.OpenConnectionAsync(token);
                    await item.GetWeather(connection, _originalService, token);
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

    public void Exception(IExceptionable callBack, Exception ex)
    {
        try
        {
            callBack.OnException(ex);
            _logger.LogError(ex, "An error occurred");
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

    public Task<WeatherForecast?> GetWeather(DbConnection connection, DateTime date, CancellationToken token = default)
    {
        return _originalService.GetWeather(connection, date, token);
    }
}
