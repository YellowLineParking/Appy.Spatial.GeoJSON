using System;
using System.Collections.Generic;
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

            if (!(token is JObject obj)) return null;
            if (!obj.GetValue("geometry", StringComparison.OrdinalIgnoreCase).HasValues) return featureOf<Geometry>(obj);
            switch (((obj.GetValue("geometry", StringComparison.OrdinalIgnoreCase) as JObject)?.GetValue("type",
                        StringComparison.OrdinalIgnoreCase) ?? null)?.Value<string>())
            {
                case "Polygon": return featureOf<Polygon>(obj);
                case "LineString": return featureOf<LineString>(obj);
                case "MultiLineString": return featureOf<MultiLineString>(obj);
                case "MultiPolygon": return featureOf<MultiPolygon>(obj);
                case "GeometryCollection": return featureOf<GeometryCollection>(obj);
                case "Point": return featureOf<Point>(obj);
                default: throw new ArgumentException("Unsupported geometry type: " + obj["type"].Value<string>(), nameof(objectType));
            }
        }

        static Feature<T> featureOf<T>(JObject input) where T : Geometry
        {
            var result = new Feature<T>();
            JsonConvert.PopulateObject(input.ToString(), result, new JsonSerializerSettings { Converters = new List<JsonConverter> { new FeatureConverter(), new GeometryConverter()}});
            return result;
        }

        public override bool CanConvert(Type objectType) => typeof(Feature).IsAssignableFrom(objectType) &&
            objectType.GenericTypeArguments.Length == 0;
    }
}