using FluentResults;
using MediatR;

namespace DatabaseApp.Application.User.Command.CreateUser;

public struct CreateUserCommand : IRequest<Result>
{
    public long TelegramId { get; init; }
    
    public string FullName { get; init; }
    
    public string GroupName { get; init; }

}