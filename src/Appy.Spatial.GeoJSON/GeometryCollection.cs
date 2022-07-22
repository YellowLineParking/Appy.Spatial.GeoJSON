namespace Appy.Spatial.GeoJSON
{
    public class GeometryCollection : Geometry
    {
        public GeometryCollection()
        {
            Type = GeoType.GeometryCollection;
        }

        public List<Geometry> Geometries { get; set; }
    }
}