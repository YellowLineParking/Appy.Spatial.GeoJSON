using System.Collections.Generic;

namespace Appy.Spatial.GeoJSON;

public class FeatureCollection
{
    public string Type { get; set; } = "FeatureCollection";
    public List<Feature> Features { get; set; }
    public dynamic Properties { get; set; }
}

public class FeatureCollection<T> where T : Geometry
{
    public string Type { get; set; } = "FeatureCollection";
    public List<Feature<T>> Features { get; set; }
    public dynamic Properties { get; set; }
}

public class FeatureCollection<TGeometry, TProperties> where TGeometry : Geometry
{
    public string Type { get; set; } = "FeatureCollection";
    public List<Feature<TGeometry, TProperties>> Features { get; set; }
    public dynamic Properties { get; set; }
}