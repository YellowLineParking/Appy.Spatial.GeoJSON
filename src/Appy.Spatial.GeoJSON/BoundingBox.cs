using System.Collections.Generic;

namespace Appy.Spatial.GeoJSON;

public class BoundingBox
{
    public LatLng TopLeft { get; set; }
    public LatLng BottomRight { get; set; }
}

public class BoundingBoxComparer : IEqualityComparer<BoundingBox>
{
    public bool Equals(BoundingBox x, BoundingBox y) =>
        x == null && y == null ||
        x != null && y != null &&
        new LatLngComparer().Equals(x.TopLeft, y.TopLeft) &&
        new LatLngComparer().Equals(x.BottomRight, y.BottomRight);

    public int GetHashCode(BoundingBox obj) =>
        obj.TopLeft?.Latitude.GetHashCode() ?? 0 +
        obj.TopLeft?.Longitude.GetHashCode() ?? 0 +
        obj.BottomRight?.Latitude.GetHashCode() ?? 0 +
        obj.BottomRight?.Longitude.GetHashCode() ?? 0;
}