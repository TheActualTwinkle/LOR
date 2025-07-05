using System.Diagnostics;
using FluentValidation;
using MediatR;

namespace DatabaseApp.Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    // ReSharper disable once StaticMemberInGenericType
    private static readonly ActivitySource ActivitySource = new("MediatR.Validation");

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity($"{typeof(TRequest).Name}.Validator");

        ValidationContext<TRequest> context = new(request);

        var failures = validators
            .Select(v => v.Validate(context))
            .SelectMany(result => result.Errors)
            .Where(failure => failure != null)
            .ToList();

        if (failures.Count != 0)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Validation failed");

            throw new ValidationException(failures);
        }

        activity?.SetStatus(ActivityStatusCode.Ok);
        activity?.Stop();
        
        return await next();
    }
}