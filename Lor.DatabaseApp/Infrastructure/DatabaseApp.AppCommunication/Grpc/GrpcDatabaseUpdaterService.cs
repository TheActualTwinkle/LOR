using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace DatabaseApp.AppCommunication.Grpc;

public class GrpcDatabaseUpdaterService : DatabaseUpdater.DatabaseUpdaterBase
{
    public static List<string> AvailableGroups = ["АВТ-218"];
    
    public override Task<Empty> SetAvailableGroups(SetAvailableGroupsRequest request, ServerCallContext context)
    {
        AvailableGroups = request.GroupNames.ToList();
        return Task.FromResult(new Empty());
    }

    public override Task<Empty> SetAvailableLabClasses(SetAvailableLabClassesRequest request, ServerCallContext context)
    {
        return Task.FromResult(new Empty());
    }
}