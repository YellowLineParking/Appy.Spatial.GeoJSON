using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Appy.Spatial.GeoJSON.TextJson;

public class FeatureConverter : JsonConverter<Feature>
{
    public override bool CanConvert(Type typeToConvert) =>
        typeof(Feature) == typeToConvert;

    public override Feature Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var readerClone = reader;

        if (readerClone.TokenType != JsonTokenType.StartObject) 
            throw new JsonException("TokenType is not StartObject");

        var propertyName = string.Empty;
        var initialDepth = readerClone.CurrentDepth + 2;
            
        while (propertyName.ToLower() != "type")
        {
            readerClone.Read();
                
            if (readerClone.CurrentDepth != initialDepth) 
                continue;
                
            if (readerClone.TokenType != JsonTokenType.PropertyName) 
                continue;

            propertyName = readerClone.GetString();
        }

        readerClone.Read();
            
        var geometryType = readerClone.GetString();

        return geometryType switch
        {
            GeoType.GeometryCollection => FeatureOf<GeometryCollection>(ref reader, options),
            GeoType.Polygon => FeatureOf<Polygon>(ref reader, options),
            GeoType.LineString => FeatureOf<LineString>(ref reader, options),
            GeoType.MultiLineString => FeatureOf<MultiLineString>(ref reader, options),
            GeoType.MultiPolygon => FeatureOf<MultiPolygon>(ref reader, options),
            GeoType.Point => FeatureOf<Point>(ref reader, options),
            _ => throw new JsonException($"Unsupported geometry type: {geometryType}")
        };
    }

    static Feature<T> FeatureOf<T>(ref Utf8JsonReader reader, JsonSerializerOptions options) where T : Geometry =>
        JsonSerializer.Deserialize<Feature<T>>(ref reader, options);
            
    public override void Write(Utf8JsonWriter writer, Feature feature, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, feature, feature.GetType(), options);
}