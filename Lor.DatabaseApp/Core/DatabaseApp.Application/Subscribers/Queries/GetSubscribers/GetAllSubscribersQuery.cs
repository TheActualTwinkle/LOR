using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Subscribers.Queries;

public record GetAllSubscribersQuery : IRequest<Result<List<SubscriberDto>>>;