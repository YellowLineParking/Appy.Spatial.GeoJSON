using System.Collections.Generic;

namespace Appy.Spatial.GeoJSON;

public class Polygon : Geometry<IList<IList<IList<double>>>>
{
    public Polygon()
        : base(GeoType.Polygon)
    {
    }

    public Polygon(IList<IList<IList<double>>> coordinates)
        : base(GeoType.Polygon, coordinates)
    {
    }
}