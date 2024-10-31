using Google.Protobuf;
using Google.Protobuf.Collections;

namespace DatabaseApp.Application.Common.ExtensionsMethods;

public static class RepeatedFiledObjectConverter
{
    public static Task<RepeatedField<T>> ToRepeatedField<T, TSource>(this IEnumerable<TSource> source, 
        Func<TSource, T> mapper) 
        where T : IMessage
    {
        RepeatedField<T> repeatedField = new();

        foreach (var item in source)
        {
            repeatedField.Add(mapper(item));
        }

        return Task.FromResult(repeatedField);
    }
}