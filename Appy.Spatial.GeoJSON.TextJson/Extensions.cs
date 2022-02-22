using System.Text.Json;

namespace Appy.Spatial.GeoJSON.TextJson
{
    public static class Extensions
    {
        public static JsonSerializerOptions UseGeoJsonConverters(this JsonSerializerOptions options)
        {
            options.Converters.Add(new FeatureConverter());
            options.Converters.Add(new GeometryConverter());
            // options.Converters.Add(new FeaturePropertiesConverter());
            return options;
        }
    }
}