using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OtelDemo.Otel;


namespace OtelDemo;

public static class ServiceExtension
{
    public static IServiceCollection AddOtel(this IServiceCollection services, WebApplicationBuilder builder)
    {
        // 定义服务名称和版本
        var serviceName = OtelUtil.ServiceName;
        var serviceVersion = OtelUtil.ServiceVersion;


        builder.Logging.AddOpenTelemetry(options =>
        {
            options
                .SetResourceBuilder(
                    ResourceBuilder.CreateDefault()
                        .AddService(serviceName));

            options.AddOtlpExporter(opt =>
            {
                opt.Endpoint = new Uri("http://localhost:4317"); // OTLP gRPC 接口
            });
        });

        services.AddOpenTelemetry()
            .ConfigureResource(resource => 
                resource.AddService(serviceName: serviceName, serviceVersion: serviceVersion)
            )
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();
                tracing.AddSource(OtelUtil.ServiceName);

                tracing.AddOtlpExporter(opt =>
                {
                    opt.Endpoint = new Uri("http://localhost:4317"); // OTLP gRPC 接口
                });
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();
                metrics.AddMeter(OtelUtil.ServiceName);

                metrics.AddOtlpExporter(opt =>
                {
                    opt.Endpoint = new Uri("http://localhost:4317");
                });
            });


        return services;
    }
}
