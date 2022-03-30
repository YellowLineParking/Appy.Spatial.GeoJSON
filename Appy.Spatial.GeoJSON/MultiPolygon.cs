namespace Appy.Spatial.GeoJSON
{
    public class MultiPolygon : Geometry<IList<IList<IList<IList<double>>>>>
    {
        public MultiPolygon()
            : base(GeoType.MultiPolygon)
        {
        }

        public MultiPolygon(IList<IList<IList<IList<double>>>> coordinates) 
            : base(GeoType.MultiPolygon, coordinates)
        {
        }

        public MultiPolygon(IEnumerable<Polygon> polygons)
            : base(GeoType.MultiPolygon, polygons.Select(p => p.Coordinates).ToList())
        {
        }
    }
}