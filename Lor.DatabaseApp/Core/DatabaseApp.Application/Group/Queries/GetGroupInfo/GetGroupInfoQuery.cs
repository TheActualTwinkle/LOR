using FluentResults;
using MediatR;

namespace DatabaseApp.Application.Group.Queries.GetGroupInfo;

public struct GetGroupInfoQuery : IRequest<Result<GroupDto>>
{
    public string? GroupName { get; init; }
    public int? GroupId { get; init; }
    
    public long? TelegramId { get; init; }
    
}