using Newtonsoft.Json;

namespace Appy.Spatial.GeoJSON.Newtonsoft;

public static class Extensions
{
    public static JsonSerializerSettings UseGeoJsonConverters(this JsonSerializerSettings options)
    {
        options.Converters.Add(new FeatureConverter());
        options.Converters.Add(new GeometryConverter());
        return options;
    }
        
    internal static T PopulateObject<T>(this JsonSerializer serializer, JsonReader jsonReader, T target)
    {
        serializer.Populate(jsonReader, target);

        if (!serializer.CheckAdditionalContent)
            return target;

        while (jsonReader.Read())
        {
            if (jsonReader.TokenType != JsonToken.Comment)
            {
                throw new JsonSerializationException("Additional text found in JSON string after finishing deserializing object.");
            }
        }
            
        return target;
    }
}