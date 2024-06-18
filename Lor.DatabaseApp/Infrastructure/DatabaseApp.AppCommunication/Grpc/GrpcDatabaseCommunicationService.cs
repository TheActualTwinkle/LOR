using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace DatabaseApp.AppCommunication.Grpc;

public class GrpcDatabaseCommunicationService : DatabaseCommunication.DatabaseCommunicationBase
{
    public override Task<GetAvailableGroupsReply> GetAvailableGroups(Empty request, ServerCallContext context)
    {
        GetAvailableGroupsReply reply = new();
        reply.IdGroupsMap.Add(0, "АВТ-218");
        return Task.FromResult(reply);
    }

    public override Task<GetAvailableLabClassesReply> GetAvailableLabClasses(GetAvailableLabClassesRequest request, ServerCallContext context)
    {
        GetAvailableLabClassesReply reply = new();
        reply.IdClassMap.Add(0, "ОС");
        return Task.FromResult(reply);
    }

    public override Task<TrySetGroupReply> TrySetGroup(TrySetGroupRequest request, ServerCallContext context)
    {
        TrySetGroupReply reply = new() { ErrorMessage = request.GroupName == "АВТ-218" ? string.Empty : "Группа не найдена или не поддерживается" };
        return Task.FromResult(reply);
    }

    public override Task<TryEnqueueInClassReply> TryEnqueueInClass(TryEnqueueInClassRequest request, ServerCallContext context)
    {
        TryEnqueueInClassReply reply = new() { StudentsQueue = { "Мойкин", "Мякинин", "Стельмах" } };
        return Task.FromResult(reply);
    }
}