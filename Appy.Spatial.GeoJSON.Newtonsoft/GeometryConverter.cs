using System;
using System.Collections.Generic;
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
            if (!CanConvert(objectType)) throw new ArgumentException("Unsupported target type: " + objectType, nameof(objectType));

            var token = JToken.Load(reader);

            if (!(token is JObject obj)) return null;

            switch (obj.GetValue("type", StringComparison.OrdinalIgnoreCase)?.Value<string>())
            {
                case "Polygon": return geometryAs<Polygon>(obj);
                case "LineString": return geometryAs<LineString>(obj);
                case "MultiLineString": return geometryAs<MultiLineString>(obj);
                case "MultiPolygon": return geometryAs<MultiPolygon>(obj);
                case "Point": return geometryAs<Point>(obj);
                case "GeometryCollection": return geometryAs<GeometryCollection>(obj);
                default: throw new ArgumentException("Unsupported geometry type: " + obj["type"].Value<string>(), nameof(objectType));
            }
        }

        static T geometryAs<T>(JObject input) where T : new()
        {
            var result = new T();
            JsonConvert.PopulateObject(input.ToString(), result, new JsonSerializerSettings{ Converters = new List<JsonConverter> { new GeometryConverter()} });
            return result;
        }

        public override bool CanConvert(Type objectType) => typeof(Geometry).IsAssignableFrom(objectType);
    }
}