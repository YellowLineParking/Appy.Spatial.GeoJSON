using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Appy.Spatial.GeoJSON.TextJson
{

    public class FeatureConverter : JsonConverter<Feature>
    {
        public override bool CanConvert(Type typeToConvert) =>
            typeof(Feature) == typeToConvert && typeToConvert.GenericTypeArguments.Length == 0;

        public override Feature Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var readerClone = reader;

            if (readerClone.TokenType != JsonTokenType.StartObject) throw new JsonException();

            var propertyName = string.Empty;
            var initialDepth = readerClone.CurrentDepth + 2;
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
                case GeoType.GeometryCollection: return JsonSerializer.Deserialize<Feature<GeometryCollection>>(ref reader, options);
                case GeoType.Polygon: return JsonSerializer.Deserialize<Feature<Polygon>>(ref reader, options);
                case GeoType.LineString: return JsonSerializer.Deserialize<Feature<LineString>>(ref reader, options);
                case GeoType.MultiLineString: return JsonSerializer.Deserialize<Feature<MultiLineString>>(ref reader, options);
                case GeoType.MultiPolygon: return JsonSerializer.Deserialize<Feature<MultiPolygon>>(ref reader, options);
                case GeoType.Point: return JsonSerializer.Deserialize<Feature<Point>>(ref reader, options);
                default: throw new ArgumentException("Unsupported geometry type: " + geoType);
            }
        }

        public override void Write(Utf8JsonWriter writer, Feature feature, JsonSerializerOptions options) =>
            JsonSerializer.Serialize(writer, feature, feature.GetType(), options);
    }
}