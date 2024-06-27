using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace DatabaseApp.AppCommunication.Grpc;

public class GrpcDatabaseService : Database.DatabaseBase
{
    private static string _userGroup = string.Empty;

    public override Task<GetUserGroupReply> GetUserGroup(GetUserGroupRequest request, ServerCallContext context)
    {
        GetUserGroupReply reply = new()
        {
            GroupName = _userGroup,
            IsFailed = string.IsNullOrEmpty(_userGroup),
            ErrorMessage = string.IsNullOrEmpty(_userGroup) ? "Вы не авторизованы. Для авторизации введите /auth <ФИО>" : string.Empty
        };
        return Task.FromResult(reply);
    }

    public override Task<GetAvailableGroupsReply> GetAvailableGroups(Empty request, ServerCallContext context)
    {
        GetAvailableGroupsReply reply = new();

        for (var i = 0; i < GrpcDatabaseUpdaterService.AvailableGroups.Count; i++)
        {
            reply.IdGroupsMap.Add(i, GrpcDatabaseUpdaterService.AvailableGroups[i]);
        }

        return Task.FromResult(reply);
    }

    public override Task<GetAvailableLabClassesReply> GetAvailableLabClasses(GetAvailableLabClassesRequest request, ServerCallContext context)
    {
        if (string.IsNullOrEmpty(_userGroup))
        {
            return Task.FromResult(new GetAvailableLabClassesReply { IsFailed = true, ErrorMessage = "Вы не авторизованы. Для авторизации введите /auth <ФИО>" });
        }
        
        GetAvailableLabClassesReply reply = new();
        reply.IdClassMap.Add(0, "Операционные Сети");
        reply.IdClassMap.Add(1, "Электроника");
        return Task.FromResult(reply);
    }

    public override Task<TrySetGroupReply> TrySetGroup(TrySetGroupRequest request, ServerCallContext context)
    {
        if (request.GroupName is "АВТ-218" or "АВТ-214")
        {
            _userGroup = request.GroupName;
            return Task.FromResult(new TrySetGroupReply { GroupName = request.GroupName });
        }
        
        TrySetGroupReply reply = new() { IsFailed = true, ErrorMessage = "Группа не найдена или не поддерживается"};
        
        return Task.FromResult(reply);
    }

    public override Task<TryEnqueueInClassReply> TryEnqueueInClass(TryEnqueueInClassRequest request, ServerCallContext context)
    {
        // TODO: Узнавать есть ли группа из request.UserId в базе данных
        if (string.IsNullOrEmpty(_userGroup))
        {
            return Task.FromResult(new TryEnqueueInClassReply { IsFailed = true, ErrorMessage = "Вы не авторизованы. Для авторизации введите /auth <ФИО>" });
        }
        
        return Task.FromResult(new TryEnqueueInClassReply { StudentsQueue = { "Кто-то", "Кто то", "Вы!" }});
    }
}