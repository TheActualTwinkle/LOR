using DatabaseApp.Application.Users;
using DatabaseApp.Domain.Repositories;
using FluentResults;
using Mapster;
using MediatR;

namespace DatabaseApp.Application.QueueEntries.Queries;

public class GetEnqueuedUsersQueryHandler(
    IUnitOfWork unitOfWork)
    : IRequestHandler<GetEnqueuedUsersQuery, Result<List<UserDto>>>
{
    public async Task<Result<List<UserDto>>> Handle(GetEnqueuedUsersQuery request, CancellationToken cancellationToken)
    {
        var users = new List<Domain.Models.User>();

        var userRepository = unitOfWork.GetRepository<IUserRepository>();
        
        foreach (var queueEntry in request.Queue)
        {
           var user = await userRepository.GetUserByFullName(queueEntry.FullName, cancellationToken);
            
            if (user is null) 
                return Result.Fail("User not found");
            
            users.Add(user);
        }
        
        return Result.Ok(users.Adapt<List<UserDto>>());
    }
}