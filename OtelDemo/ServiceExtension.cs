using OpenTelemetry.Exporter;
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

        // Microsoft.Extensions.Logging 使用下面 builder.Logging.AddOpenTelemetry
        // serilog 需要使用 Serilog.Sinks.OpenTelemetry
        builder.Logging.AddOpenTelemetry(options =>
        {
            options
                .SetResourceBuilder(
                    ResourceBuilder.CreateDefault()
                        .AddService(serviceName));

            options.AddOtlpExporter(opt =>
            {
                opt.Endpoint = new Uri("http://localhost:4318/v1/logs");  // OTLP 接口
                // opt.Endpoint = new Uri("http://loki:3100/otlp"); // OTLP loki 接口
                opt.Protocol = OtlpExportProtocol.HttpProtobuf;
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
                    //opt.Endpoint = new Uri("http://localhost:4318/v1/traces");  // OTLP 接口
                    opt.Endpoint = new Uri("http://localhost:4317");  // OTLP 接口
                    opt.Protocol = OtlpExportProtocol.Grpc;

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
                    opt.Endpoint = new Uri("http://localhost:4318/v1/metrics");  // OTLP prometheus 接口
                    opt.Protocol = OtlpExportProtocol.HttpProtobuf;
                });

                // OpenTelemetry.Exporter.Prometheus.AspNetCore
                //metrics.AddPrometheusExporter(opt =>
                //{
                //});
            });


        return services;
    }
}
