
using OtelDemo;
using OtelDemo.Otel;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

#region serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Services.AddSerilog();
#endregion

// 配置 OpenTelemetry
builder.Services.AddOtel(builder);


builder.Services.AddScoped<IWeatherService, WeatherService>();


// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/weatherforecast", async (IWeatherService weatherService) =>
{
   return await weatherService.GetWeatherForecastAsync();
})
.WithName("GetWeatherForecast")
.WithOpenApi();


app.Lifetime.ApplicationStopping.Register(() =>
{
    Console.WriteLine("Application stopping.");
    OtelUtil.Dispose();
});

app.Run();
