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
                throw new ArgumentException("Unsupported target type: " + objectType, nameof(objectType));

            var token = JToken.Load(reader);

            if (token is not JObject obj) 
                return null;

            var geometryToken = obj.GetValue("geometry", StringComparison.OrdinalIgnoreCase);

            if (!geometryToken!.HasValues) 
                return FeatureOf<Geometry>(obj, serializer);

            var geometryType = (geometryToken as JObject)?.GetValue("type", StringComparison.OrdinalIgnoreCase)?.Value<string>();

            return geometryType switch
            {
                "Polygon" => FeatureOf<Polygon>(obj, serializer),
                "LineString" => FeatureOf<LineString>(obj, serializer),
                "MultiLineString" => FeatureOf<MultiLineString>(obj, serializer),
                "MultiPolygon" => FeatureOf<MultiPolygon>(obj, serializer),
                "GeometryCollection" => FeatureOf<GeometryCollection>(obj, serializer),
                "Point" => FeatureOf<Point>(obj, serializer),
                _ => throw new ArgumentException($"Unsupported geometry type: {obj["type"].Value<string>()}", nameof(objectType))
            };
        }

        static Feature<T> FeatureOf<T>(JToken input, JsonSerializer serializer) where T : Geometry
        {
            using var jsonReader = input.CreateReader();
            return serializer.PopulateObject(jsonReader, new Feature<T>());
        }

        public override bool CanConvert(Type objectType) => 
            typeof(Feature).IsAssignableFrom(objectType) && objectType.GenericTypeArguments.Length == 0;
    }
}