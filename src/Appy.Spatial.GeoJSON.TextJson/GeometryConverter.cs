using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Appy.Spatial.GeoJSON.TextJson;

public class GeometryConverter : JsonConverter<Geometry>
{
    public override bool CanConvert(Type typeToConvert) =>
        typeof(Geometry) == typeToConvert;
        
    public override Geometry Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var readerClone = reader;
            
        if (readerClone.TokenType != JsonTokenType.StartObject) 
            throw new JsonException("TokenType is not StartObject");

        var propertyName = string.Empty;
        var initialDepth = readerClone.CurrentDepth + 1;
            
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
            GeoType.GeometryCollection => GeometryAs<GeometryCollection>(ref reader, options),
            GeoType.Polygon => GeometryAs<Polygon>(ref reader, options),
            GeoType.LineString => GeometryAs<LineString>(ref reader, options),
            GeoType.MultiLineString => GeometryAs<MultiLineString>(ref reader, options),
            GeoType.MultiPolygon => GeometryAs<MultiPolygon>(ref reader, options),
            GeoType.Point => GeometryAs<Point>(ref reader, options),
            _ => throw new JsonException($"Unsupported geometry type: {geometryType}")
        };
    }
        
    public override void Write(Utf8JsonWriter writer, Geometry geometry, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, geometry, geometry.GetType(), options);
        
    static TGeometry GeometryAs<TGeometry>(ref Utf8JsonReader reader, JsonSerializerOptions options) where TGeometry : Geometry => 
        JsonSerializer.Deserialize<TGeometry>(ref reader, options);
}