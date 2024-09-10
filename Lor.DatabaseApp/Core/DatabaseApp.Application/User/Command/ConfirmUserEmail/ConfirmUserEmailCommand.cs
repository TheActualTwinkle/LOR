using FluentResults;
using MediatR;

namespace DatabaseApp.Application.User.Command.ConfirmUserEmail;

public struct ConfirmUserEmailCommand : IRequest<Result>
{
    public required Guid TokenIdentifier { get; init; }
}