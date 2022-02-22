using Newtonsoft.Json;

namespace Appy.Spatial.GeoJSON.Newtonsoft
{
    public static class Extensions
    {
        public static JsonSerializerSettings UseGeoJsonConverters(this JsonSerializerSettings options)
        {
            options.Converters.Add(new FeatureConverter());
            options.Converters.Add(new GeometryConverter());
            return options;
        }
    }
}