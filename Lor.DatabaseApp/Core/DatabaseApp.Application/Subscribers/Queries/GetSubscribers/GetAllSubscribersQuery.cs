using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Subscriber.Queries.GetSubscribers;

public record GetAllSubscribersQuery : IRequest<Result<List<SubscriberDto>>>;