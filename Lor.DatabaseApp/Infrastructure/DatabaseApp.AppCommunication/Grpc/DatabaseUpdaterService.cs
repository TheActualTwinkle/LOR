using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace DatabaseApp.AppCommunication.Grpc;

public class DatabaseUpdaterService : DatabaseUpdater.DatabaseUpdaterBase
{
    public override Task<Empty> UpdateAvailableGroups(UpdateAvailableGroupsRequest request, ServerCallContext context)
    {
        return base.UpdateAvailableGroups(request, context);
    }
    
    public override Task<Empty> UpdateAvailableLabClasses(UpdateAvailableLabClassesRequest request, ServerCallContext context)
    {
        return base.UpdateAvailableLabClasses(request, context);
    }
}