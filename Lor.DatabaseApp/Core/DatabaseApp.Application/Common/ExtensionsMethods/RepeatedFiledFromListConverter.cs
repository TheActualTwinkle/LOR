using Google.Protobuf.Collections;

namespace DatabaseApp.Application.Common.ExtensionsMethods;

public static class RepeatedFiledFromListConverter
{
    public static async Task FromList<T>(this RepeatedField<T> repeatedField, List<T> list) 
    {
        foreach (var item in list)
        {
            repeatedField.Add(item);
        }

        await Task.FromResult(repeatedField);
    } 
}