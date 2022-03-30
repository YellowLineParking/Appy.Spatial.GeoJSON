namespace Appy.Spatial.GeoJSON
{
    public class Point : Geometry<IList<double>>
    {
        public Point()
            : base(GeoType.Point)
        {
        }

        public Point(IList<double> coordinates)
            : base(GeoType.Point, coordinates)
        {
        }
        
        public LatLng ToLatLng() => new() {Latitude = Coordinates[1], Longitude = Coordinates[0]};
    }
}