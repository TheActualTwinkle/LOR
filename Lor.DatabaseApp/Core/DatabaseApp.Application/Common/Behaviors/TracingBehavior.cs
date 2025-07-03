using System.Diagnostics;
using MediatR;

namespace DatabaseApp.Application.Common.Behaviors;

public class TracingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    // ReSharper disable once StaticMemberInGenericType
    private static readonly ActivitySource ActivitySource = new("MediatR");

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity($"{typeof(TRequest).Name}Handler");

        if (activity == null)
            return await next();

        try
        {
            activity.SetTag("request.type", typeof(TRequest).FullName);

            var response = await next();
            activity.SetStatus(ActivityStatusCode.Ok);

            return response;
        }
        catch (Exception ex)
        {
            activity.SetStatus(ActivityStatusCode.Error, ex.Message);

            throw;
        }
    }
}