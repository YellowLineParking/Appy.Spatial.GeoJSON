using System.Collections.Generic;

namespace Appy.Spatial.GeoJSON;

public class LineString : Geometry<IList<IList<double>>>
{
    public LineString()
        : base(GeoType.LineString)
    {
    }

    public LineString(IList<IList<double>> coordinates)
        : base(GeoType.LineString, coordinates)
    {
    }
}