using System.IO;
using Newtonsoft.Json;

namespace Appy.Spatial.GeoJSON.Newtonsoft
{
    public static class JsonSerializerExtensions
    {
        public static T PopulateObject<T>(this JsonSerializer serializer, string value, T target)
        {
            using (JsonReader jsonReader = new JsonTextReader(new StringReader(value)))
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
            }
            
            return target;
        }
    }
}