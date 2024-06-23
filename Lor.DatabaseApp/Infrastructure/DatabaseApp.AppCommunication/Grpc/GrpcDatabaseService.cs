using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace DatabaseApp.AppCommunication.Grpc;

public class GrpcDatabaseService : Database.DatabaseBase
{
    private static bool _isUserInGroup;

    public override Task<IsUserInGroupReply> IsUserInGroup(IsUserInGroupRequest request, ServerCallContext context)
    {
        return Task.FromResult(new IsUserInGroupReply { IsUserInGroup = _isUserInGroup });
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
        if (_isUserInGroup == false)
        {
            return Task.FromResult(new GetAvailableLabClassesReply { IsFailed = true, ErrorMessage = "Вы не состоите ни в одной группе. Установите группу командой /setgroup" });
        }
        
        GetAvailableLabClassesReply reply = new();
        reply.IdClassMap.Add(0, "Операционные Сети");
        reply.IdClassMap.Add(1, "Электроника");
        return Task.FromResult(reply);
    }

    public override Task<TrySetGroupReply> TrySetGroup(TrySetGroupRequest request, ServerCallContext context)
    {
        if (request.GroupId is 0 or 1)
        {
            _isUserInGroup = true;
            return Task.FromResult(new TrySetGroupReply() { GroupName = request.GroupId == 0 ? "АВТ-218" : "АВТ-214"});
        }
        
        TrySetGroupReply reply = new() { IsFailed = true, ErrorMessage = "Группа не найдена или не поддерживается"};
        
        return Task.FromResult(reply);
    }

    public override Task<TryEnqueueInClassReply> TryEnqueueInClass(TryEnqueueInClassRequest request, ServerCallContext context)
    {
        // TODO: Узнавать есть ли группа из request.UserId в базе данных
        if (_isUserInGroup == false)
        {
            return Task.FromResult(new TryEnqueueInClassReply { IsFailed = true, ErrorMessage = "Вы не состоите ни в одной группе. Установите группу командой /setgroup" });
        }
        
        return Task.FromResult(new TryEnqueueInClassReply() { StudentsQueue = { "Кто-то", "Кто то", "Вы!" }});
    }
}