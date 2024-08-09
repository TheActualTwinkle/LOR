﻿using DatabaseApp.AppCommunication.Grpc;
using FluentResults;
using TelegramBotApp.AppCommunication.Data;

namespace TelegramBotApp.AppCommunication.Interfaces;

public interface IDatabaseCommunicationClient : ICommunicationClient
{
    Task<Result<string>> GetUserGroup(long userId, CancellationToken cancellationToken = default); 
    
    Task<Result<Dictionary<int, string>>> GetAvailableGroups(CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<ClassInformation>>> GetAvailableLabClasses(long userId, CancellationToken cancellationToken = default);
    
    Task<Result<string>> TrySetGroup(long userId, string groupName, string fullName, CancellationToken cancellationToken = default);
    Task<Result<EnqueueInClassResult>> EnqueueInClass(int cassId, long userId, CancellationToken cancellationToken = default);
}