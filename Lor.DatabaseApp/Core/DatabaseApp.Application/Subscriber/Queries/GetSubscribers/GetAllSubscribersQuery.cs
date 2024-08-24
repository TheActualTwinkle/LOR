using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Subscriber.Queries.GetSubscribers;

public class GetAllSubscribersQuery : IRequest<Result<List<SubscriberDto>>>;