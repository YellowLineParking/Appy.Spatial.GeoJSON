using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Appy.Spatial.GeoJSON.Newtonsoft
{
    public class GeometryConverter : JsonConverter
    {
        public override bool CanWrite => false;
        
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();
        
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (!CanConvert(objectType)) 
                throw new JsonException($"Unsupported target type {objectType}");

            var token = JToken.Load(reader);

            if (token is not JObject obj) 
                return null;

            return obj.GetValue("type", StringComparison.OrdinalIgnoreCase)?.Value<string>() switch
            {
                "Polygon" => GeometryAs<Polygon>(obj, serializer),
                "LineString" => GeometryAs<LineString>(obj, serializer),
                "MultiLineString" => GeometryAs<MultiLineString>(obj, serializer),
                "MultiPolygon" => GeometryAs<MultiPolygon>(obj, serializer),
                "Point" => GeometryAs<Point>(obj, serializer),
                "GeometryCollection" => GeometryAs<GeometryCollection>(obj, serializer),
                _ => throw new JsonException($"Unsupported geometry type {obj["type"].Value<string>()}")
            };
        }

        static T GeometryAs<T>(JToken input, JsonSerializer serializer) where T : new()
        {
            using var jsonReader = input.CreateReader();
            return serializer.PopulateObject(jsonReader, new T());
        }

        public override bool CanConvert(Type objectType) => 
            typeof(Geometry).IsAssignableFrom(objectType);
    }
}