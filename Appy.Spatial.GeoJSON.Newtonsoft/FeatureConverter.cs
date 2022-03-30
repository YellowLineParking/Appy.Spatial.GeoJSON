using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Appy.Spatial.GeoJSON.Newtonsoft
{
    public class FeatureConverter : JsonConverter
    {
        public override bool CanWrite => false;
        
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();
        
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (!CanConvert(objectType)) 
                throw new JsonException($"Unsupported target type: {objectType}");

            var token = JToken.Load(reader);

            if (token is not JObject obj) 
                return null;

            var geometryToken = obj.GetValue("geometry", StringComparison.OrdinalIgnoreCase);

            if (!geometryToken!.HasValues) 
                return FeatureOf<Geometry>(obj, serializer);

            var geometryType = (geometryToken as JObject)?.GetValue("type", StringComparison.OrdinalIgnoreCase)?.Value<string>();

            return geometryType switch
            {
                GeoType.GeometryCollection => FeatureOf<GeometryCollection>(obj, serializer),
                GeoType.Polygon => FeatureOf<Polygon>(obj, serializer),
                GeoType.LineString => FeatureOf<LineString>(obj, serializer),
                GeoType.MultiLineString => FeatureOf<MultiLineString>(obj, serializer),
                GeoType.MultiPolygon => FeatureOf<MultiPolygon>(obj, serializer),
                GeoType.Point => FeatureOf<Point>(obj, serializer),
                _ => throw new JsonException($"Unsupported geometry type: {geometryType}")
            };
        }
        
        public override bool CanConvert(Type objectType) => 
            typeof(Feature) == objectType;

        static Feature<T> FeatureOf<T>(JToken input, JsonSerializer serializer) where T : Geometry
        {
            using var jsonReader = input.CreateReader();
            return serializer.PopulateObject(jsonReader, new Feature<T>());
        }
    }
}