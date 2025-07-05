using System.Diagnostics.Metrics;

namespace DatabaseApp.WebApi.Middleware.Grpc;

public static class GrpcMetrics
{
    public static readonly Meter Meter = new("Lor.DatabaseApp.GrpcServiceMetrics");

    // ReSharper disable once UnusedMember.Global
    public static readonly Counter<long> TotalRequestsCounter = Meter.CreateCounter<long>(
        "total_requests",
        description: "Total number of gRPC requests over the application lifetime");

    // ReSharper disable once UnusedMember.Global
    public static readonly ObservableCounter<long> CurrentCallsCounter = Meter.CreateObservableCounter(
        "current_requests",
        () => Interlocked.Exchange(ref _currentCalls, 0),
        description: "Number of gRPC requests since the last metric poll (resets after reporting)");

    private static long _currentCalls;

    public static void IncrementCurrentCalls() =>
        Interlocked.Increment(ref _currentCalls);
}