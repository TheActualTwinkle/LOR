using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace GroupScheduleApp.Grpc;

public class GroupScheduleService : GroupSchedule.GroupScheduleBase
{
    public override Task<GetAvailableGroupsReply> GetAvailableGroups(Empty request, ServerCallContext context)
    {
        GetAvailableGroupsReply reply = new();
        reply.IdGroupsMap.Add(0, "АТВ-218");
        return Task.FromResult(reply);
    }

    public override Task<GetAvailableLabClassesReply> GetAvailableLabClasses(GetAvailableLabClassesRequest request, ServerCallContext context)
    {
        GetAvailableLabClassesReply reply = new();
        reply.IdClassMap.Add(0, "ОС");
        return Task.FromResult(reply);
    }
}