using Google.Protobuf.Collections;

namespace DatabaseApp.Application.Common.Converters;

public static class MapFieldConverter
{
    public static Task FromDictionary(this MapField<int, string> reply, Dictionary<int, string> classes)
    {
        foreach (var item in classes) reply.Add(item.Key, item.Value);

        return Task.CompletedTask;
    }
}