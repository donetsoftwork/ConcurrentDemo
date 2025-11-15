using Concurrent;
using Hand.ConcurrentCollections;
using Hand.Job;
using Weather.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddMySqlDataSource("myWeather");

// Add services to the container.
const string weatherKey = "SafeWeather";
RegistProcessor(builder.Services, weatherKey, 32);
RegistServices(builder.Services);

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();
// 激活手搓线程池
_ = app.Services.GetRequiredKeyedService<ReduceJobService<WeatherWarpper>>(weatherKey);
app.Run();


static void RegistServices(IServiceCollection services)
{
    services.AddSingleton<IWeatherService, WeatherService>()
        .AddControllers();
}
/// <summary>
/// 注册线程池
/// </summary>
/// <param name="services"></param>
/// <param name="name"></param>
/// <param name="concurrency"></param>
/// <returns></returns>
static void RegistProcessor(IServiceCollection services, string name, uint concurrency)
{
    var options = new ReduceOptions { ConcurrencyLevel = concurrency, ItemLife = TimeSpan.FromSeconds(3), ReduceTime = TimeSpan.FromMilliseconds(1L) };
    var queue = new ConcurrentQueueAdapter<WeatherWarpper>();
    services.AddKeyedSingleton(name, (sp, key) => new WeatherProcessor(queue, sp.GetRequiredService<IWeatherService>(), TimeSpan.FromSeconds(10)))
        .AddKeyedSingleton(name, (sp, key) => options.CreateJob(queue, sp.GetRequiredKeyedService<WeatherProcessor>(key)));
    services.AddKeyedSingleton<IWeatherService>(name, (sp, key) => sp.GetRequiredKeyedService<WeatherProcessor>(key));
}