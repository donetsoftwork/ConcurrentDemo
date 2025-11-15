using Weather.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddMySqlDataSource("myWeather");
// Add services to the container.
builder.Services.AddSingleton<IWeatherService, WeatherService>()
    .AddControllers();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();
app.Run();
