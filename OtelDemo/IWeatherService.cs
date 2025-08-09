using OtelDemo.Otel;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace OtelDemo;


internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}


internal interface IWeatherService
{
    public Task<WeatherForecast[]> GetWeatherForecastAsync();
}


class WeatherService : IWeatherService
{

    private static string[] summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };
    private static Counter<int> successCounter = OtelUtil.Meter.CreateCounter<int>("GetWeatherForecast.count", description: "Number of successful responses");

    private readonly ILogger<WeatherService> _logger;

    public WeatherService(ILogger<WeatherService> logger)
    {
        _logger = logger;
    }

    public Task<WeatherForecast[]> GetWeatherForecastAsync()
    {
        _logger.LogInformation("GetWeatherForecastAsync begin.");
        successCounter.Add(1);


        //var activity = Activity.Current;
        // 不能使用 CreateActivity，
        // 需要使用 StartActivity 来开始一个新的活动
        using var activity = OtelUtil.ActivitySource.StartActivity("GetWeatherForecast", ActivityKind.Internal);


        var forecast = Enumerable.Range(1, 5).Select(index =>
           new WeatherForecast
           (
               DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
               Random.Shared.Next(-20, 55),
               summaries[Random.Shared.Next(summaries.Length)]
           ))
           .ToArray();
        try
        {

            // 模拟随机异常
            var r = new Random();
            var value = r.Next(1, 11);
            activity?.AddEvent(new ActivityEvent("random-exception"));
            activity?.SetTag("WeatherService.Tag.Random.Value", value);
            activity?.AddBaggage("WeatherService.Baggage.Random.Value", value.ToString());
            _logger.LogInformation("Random.Value is:{Random}", value);

            if (value > 8)
            {
                throw new Exception("模拟异常");
            }

            activity?.SetStatus(ActivityStatusCode.Ok, "success");
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Error Message:" + ex.Message);
            activity?.AddException(ex);
        }

        _logger.LogInformation("GetWeatherForecastAsync end.");
        return Task.FromResult(forecast);
    }
}

