using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace OtelDemo.Otel;

internal class OtelUtil
{
    internal const string ServiceName = "OtelDemo.Server";
    internal const string ServiceVersion = "1.0.0";
    public static ActivitySource ActivitySource { get; }
    public static Meter Meter { get; }


    static OtelUtil()
    {
        ActivitySource = new ActivitySource(ServiceName, ServiceVersion);
        Meter = new Meter(ServiceName, ServiceVersion);
    }

    public static void Dispose()
    {
        ActivitySource.Dispose();
        Meter.Dispose();
    }
}
