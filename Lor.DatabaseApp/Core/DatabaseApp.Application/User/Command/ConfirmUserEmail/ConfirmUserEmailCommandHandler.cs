using DatabaseApp.AppCommunication.Grpc;
using DatabaseApp.Application.Dto;
using DatabaseApp.Application.Group.Command.CreateGroup;
using DatabaseApp.Caching;
using DatabaseApp.Caching.Interfaces;
using DatabaseApp.Domain.Repositories;
using FluentResults;
using MediatR;

namespace DatabaseApp.Application.User.Command.ConfirmUserEmail;

public class ConfirmUserEmailCommandHandler(
    ISender mediator,
    IUnitOfWork unitOfWork,
    ICacheService cacheService) : IRequestHandler<ConfirmUserEmailCommand, Result>
{
    public async Task<Result> Handle(ConfirmUserEmailCommand request, CancellationToken cancellationToken)
    {
        var token = await cacheService.GetAsync<EmailToken>(Constants.TokenPrefix + request.TokenIdentifier,
            cancellationToken);

        if (token is null) return Result.Fail("Token not found");
        if (token.ExpirationDate.ToDateTime() < DateTime.UtcNow) return Result.Fail("Token expired");

        var emailUserDto = await cacheService.GetAsync<EmailUserDto>(Constants.UserPrefix + request.TokenIdentifier,
            cancellationToken);

        if (emailUserDto is null) return Result.Fail("User not found");

        await Task.WhenAll(
            cacheService.RemoveAsync(Constants.TokenPrefix + request.TokenIdentifier, cancellationToken),
            cacheService.RemoveAsync(Constants.TokenPrefix + emailUserDto.TelegramId, cancellationToken), // delete mock
            cacheService.RemoveAsync(Constants.UserPrefix + request.TokenIdentifier, cancellationToken),
            mediator.Send(new CreateGroupCommand { GroupName = emailUserDto.GroupName }, cancellationToken));

        var group = await unitOfWork.GroupRepository.GetGroupByGroupName(emailUserDto.GroupName, cancellationToken);

        if (group is null) return Result.Fail("Group not found");

        await unitOfWork.UserRepository.AddAsync(new()
        {
            FullName = await emailUserDto.FullName.FormatFio(),
            TelegramId = emailUserDto.TelegramId,
            Email = emailUserDto.Email,
            IsEmailConfirmed = true,
            Group = group
        }, cancellationToken);

        await unitOfWork.SaveDbChangesAsync(cancellationToken);

        return Result.Ok();
    }
}