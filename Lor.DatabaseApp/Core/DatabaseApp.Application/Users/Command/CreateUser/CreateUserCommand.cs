using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Users.Command.CreateUser;

public record CreateUserCommand : IRequest<Result>
{
    public required long TelegramId { get; init; }
    
    public required string FullName { get; init; }
    
    public required string GroupName { get; init; }

}