namespace Appy.Spatial.GeoJSON
{
    
    public abstract class Feature
    {
        public string Type { get; set; } = GeoType.Feature;
        public string Id { get; set; }
    }

    public class Feature<TGeometry> : Feature where TGeometry : Geometry
    {
        public TGeometry Geometry { get; set; }
    }
    public class Feature<TGeometry, TProperties> : Feature<TGeometry> where TGeometry : Geometry
    {
        public Feature()
        {
            
        }
        public Feature(string id, TGeometry geometry, TProperties properties)
        {
            Id = id;
            Geometry = geometry;
            Properties = properties;
        }
        public TProperties Properties { get; set; }
    }
}