using Google.Protobuf;
using Google.Protobuf.Collections;

namespace DatabaseApp.Application.Common.ExtensionsMethods;

public static class MapFieldFromDictionaryConverterAsync
{
    public static async Task<MapField<TKey, TValue>> FromDictionary<TKey, TValue>(this MapField<TKey, TValue> mapField, 
        IDictionary<TKey, TValue> dictionary)  
    {
        foreach (var (key, value) in dictionary)
        {
            mapField[key] = value;
        }

        return await Task.FromResult(mapField);
    } 
}