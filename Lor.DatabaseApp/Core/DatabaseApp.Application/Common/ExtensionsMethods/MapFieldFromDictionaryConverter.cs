using Google.Protobuf.Collections;

namespace DatabaseApp.Application.Common.ExtensionsMethods;

public static class MapFieldFromDictionaryConverter
{
    public static async Task FromDictionary<TKey, TValue>(this MapField<TKey, TValue> mapField,
        IDictionary<TKey, TValue> dictionary)  
    {
        foreach (var (key, value) in dictionary)
        {
            mapField[key] = value;
        }

        await Task.FromResult(mapField);
    } 
}