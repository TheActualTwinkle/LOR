using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Subscriber.Queries;

public record GetAllSubscribersQuery : IRequest<Result<List<SubscriberDto>>>;