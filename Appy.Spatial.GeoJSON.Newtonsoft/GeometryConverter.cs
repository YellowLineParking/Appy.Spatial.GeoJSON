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
            var token = JToken.Load(reader);

            if (token is not JObject obj) 
                return null;

            var geometryType = obj.GetValue("type", StringComparison.OrdinalIgnoreCase)?.Value<string>();
                
            return geometryType switch
            {
                GeoType.GeometryCollection => GeometryAs<GeometryCollection>(obj, serializer),
                GeoType.Polygon => GeometryAs<Polygon>(obj, serializer),
                GeoType.LineString => GeometryAs<LineString>(obj, serializer),
                GeoType.MultiLineString => GeometryAs<MultiLineString>(obj, serializer),
                GeoType.MultiPolygon => GeometryAs<MultiPolygon>(obj, serializer),
                GeoType.Point => GeometryAs<Point>(obj, serializer),
                _ => throw new JsonException($"Unsupported geometry type {geometryType}")
            };
        }
        
        public override bool CanConvert(Type objectType) => 
            typeof(Geometry) == objectType;

        static T GeometryAs<T>(JToken input, JsonSerializer serializer) where T : new()
        {
            using var jsonReader = input.CreateReader();
            return serializer.PopulateObject(jsonReader, new T());
        }
    }
}