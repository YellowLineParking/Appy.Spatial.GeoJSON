using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Appy.Spatial.GeoJSON.TextJson
{
    public class GeometryConverter : JsonConverter<Geometry>
    {
        public override bool CanConvert(Type typeToConvert) =>
            typeof(Geometry) == typeToConvert;

        public override Geometry Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var readerClone = reader;
            
            if (readerClone.TokenType != JsonTokenType.StartObject) throw new JsonException();

            var propertyName = string.Empty;
            var initialDepth = readerClone.CurrentDepth + 1;
            while (propertyName.ToLower() != "type")
            {
                readerClone.Read();
                if (readerClone.CurrentDepth != initialDepth) continue;
                
                if (readerClone.TokenType != JsonTokenType.PropertyName) continue;

                propertyName = readerClone.GetString();
            }

            readerClone.Read();
            var geoType = readerClone.GetString();

            switch (geoType)
            {
                case GeoType.GeometryCollection: return JsonSerializer.Deserialize<GeometryCollection>(ref reader, options);
                case GeoType.Polygon: return JsonSerializer.Deserialize<Polygon>(ref reader, options);
                case GeoType.LineString: return JsonSerializer.Deserialize<LineString>(ref reader, options);
                case GeoType.MultiLineString: return JsonSerializer.Deserialize<MultiLineString>(ref reader, options);
                case GeoType.MultiPolygon: return JsonSerializer.Deserialize<MultiPolygon>(ref reader, options);
                case GeoType.Point: return JsonSerializer.Deserialize<Point>(ref reader, options);
                default: throw new ArgumentException("Unsupported geometry type: " + geoType);
            }
        }

        public override void Write(Utf8JsonWriter writer, Geometry geometry, JsonSerializerOptions options) =>
            JsonSerializer.Serialize(writer, geometry, geometry.GetType(), options);
    }
}