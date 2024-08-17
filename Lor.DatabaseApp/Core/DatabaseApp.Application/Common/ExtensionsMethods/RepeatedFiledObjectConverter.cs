using Google.Protobuf;
using Google.Protobuf.Collections;

namespace DatabaseApp.Application.Common.ExtensionsMethods;

public static class RepeatedFiledObjectConverter
{
    public static Task<RepeatedField<T>> FromList<T, TSource>(this RepeatedField<T> repeatedField, 
        List<TSource> sourceList, 
        Func<TSource, T> mapper) 
        where T : IMessage
    {
        foreach (var sourceItem in sourceList)
        {
            var targetItem = mapper(sourceItem);
            repeatedField.Add(targetItem);
        }

        return Task.FromResult(repeatedField);
    }
}