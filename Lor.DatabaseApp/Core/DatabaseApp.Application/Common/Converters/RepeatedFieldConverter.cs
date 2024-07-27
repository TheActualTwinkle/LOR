using Google.Protobuf.Collections;

namespace DatabaseApp.Application.Common.Converters;

public static class RepeatedFieldConverter
{
    public static Task FromList(this RepeatedField<string> reply, List<string> queueList)
    {
        foreach (string? item in queueList) reply.Add(item);

        return Task.CompletedTask;
    }
}