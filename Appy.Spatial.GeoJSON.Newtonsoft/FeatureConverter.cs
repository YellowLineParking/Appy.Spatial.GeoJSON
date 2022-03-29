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
            if (!CanConvert(objectType)) throw new ArgumentException("Unsupported target type: " + objectType, nameof(objectType));

            var token = JToken.Load(reader);

            if (!(token is JObject obj)) 
                return null;

            var geometryToken = obj.GetValue("geometry", StringComparison.OrdinalIgnoreCase);

            if (!geometryToken.HasValues) 
                return featureOf<Geometry>(obj, serializer);
            
            switch ((geometryToken as JObject)?.GetValue("type", StringComparison.OrdinalIgnoreCase)?.Value<string>())
            {
                case "Polygon": return featureOf<Polygon>(obj, serializer);
                case "LineString": return featureOf<LineString>(obj, serializer);
                case "MultiLineString": return featureOf<MultiLineString>(obj, serializer);
                case "MultiPolygon": return featureOf<MultiPolygon>(obj, serializer);
                case "GeometryCollection": return featureOf<GeometryCollection>(obj, serializer);
                case "Point": return featureOf<Point>(obj, serializer);
                default: throw new ArgumentException($"Unsupported geometry type: {obj["type"].Value<string>()}", nameof(objectType));
            }
        }

        static Feature<T> featureOf<T>(JObject input, JsonSerializer serializer) where T : Geometry => 
            serializer.PopulateObject(input.ToString(), new Feature<T>());

        public override bool CanConvert(Type objectType) => typeof(Feature).IsAssignableFrom(objectType) &&
                                                            objectType.GenericTypeArguments.Length == 0;
    }
}