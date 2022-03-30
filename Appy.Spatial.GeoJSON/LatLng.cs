namespace Appy.Spatial.GeoJSON
{
    public class LatLng
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public Point AsPoint() => new(new[] { Longitude, Latitude });
    }

    public class LatLngComparer : IEqualityComparer<LatLng>
    {
        public bool Equals(LatLng x, LatLng y) =>
            x == null && y == null ||
            x != null && y != null && x.Latitude.Equals(y.Latitude) && x.Longitude.Equals(y.Latitude);
        
        public int GetHashCode(LatLng obj) => 
            obj.Latitude.GetHashCode() + 
            obj.Longitude.GetHashCode();
    }
}