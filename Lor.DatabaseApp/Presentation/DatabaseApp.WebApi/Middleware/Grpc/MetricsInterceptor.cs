using Grpc.Core;
using Grpc.Core.Interceptors;

namespace DatabaseApp.WebApi.Middleware.Grpc;

public class MetricsInterceptor : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        GrpcMetrics.TotalRequestsCounter.Add(1);
        GrpcMetrics.IncrementCurrentCalls();

        return await continuation(request, context);
    }
}