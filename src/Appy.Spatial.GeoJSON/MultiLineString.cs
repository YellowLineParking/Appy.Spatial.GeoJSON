namespace Appy.Spatial.GeoJSON
{
    public class MultiLineString : Geometry<IList<IList<IList<double>>>>
    {
        public MultiLineString() 
            : base(GeoType.MultiLineString)
        {
        }

        public MultiLineString(IList<IList<IList<double>>> coordinates) 
            : base(GeoType.MultiLineString, coordinates)
        {
        }

        public MultiLineString(IList<LineString> lineStrings) 
            : base(GeoType.MultiLineString, lineStrings.Select(ls => ls.Coordinates).ToList())
        {
            
        }
    }
}