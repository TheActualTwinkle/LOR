using Google.Protobuf.Collections;

namespace DatabaseApp.Application.Common.ExtensionsMethods;

public static class RepeatedFieldSimpleConverter
{
    public static Task<RepeatedField<T>> FromList<T>(this RepeatedField<T> repeatedField, List<T> list)
    {
        foreach (var item in list) repeatedField.Add(item);

        return Task.FromResult(repeatedField);
    }
}