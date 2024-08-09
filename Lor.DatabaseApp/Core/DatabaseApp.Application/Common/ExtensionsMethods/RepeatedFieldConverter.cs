using Google.Protobuf;
using Google.Protobuf.Collections;

namespace DatabaseApp.Application.Common.Converters;

public static class RepeatedFieldConverter
{
    public static async Task ToRepeatedFieldAsync<T>(this IEnumerable<T> list) where T : IMessage
    {
        RepeatedField<T> repeatedField = new RepeatedField<T>();
        
        repeatedField.AddRange(list);

        await Task.FromResult(repeatedField);
    }
}