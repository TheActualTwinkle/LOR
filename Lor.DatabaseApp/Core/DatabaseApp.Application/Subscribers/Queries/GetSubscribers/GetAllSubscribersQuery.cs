using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Subscriber.Queries.GetSubscribers;

public struct GetAllSubscribersQuery : IRequest<Result<List<SubscriberDto>>>;