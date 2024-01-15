namespace Appy.Spatial.GeoJSON;

public class Crs
{
    public string Type { get; set; }
    public CrsProperties Properties { get; set; }
        
    public static readonly Crs Wgs84 = new()
    {
        Type = "name",
        Properties = new CrsProperties
        {
            Name = "urn:ogc:def:crs:OGC:1.3:CRS84"
        }
    };

    public static readonly Crs Osgb36 = new()
    {
        Type = "name",
        Properties = new CrsProperties
        {
            Name = "urn:ogc:def:crs:EPSG::27700"
        }
    };

    public static readonly Crs Etrs89 = new()
    {
        Type = "name",
        Properties = new CrsProperties
        {
            Name = "urn:ogc:def:crs:EPSG::4258"
        }
    };

    public static readonly Crs Epsg3857 = new()
    {
        Type = "name",
        Properties = new CrsProperties
        {
            Name = "urn:ogc:def:crs:EPSG::3857"
        }
    };
}

public class CrsProperties
{
    public string Name { get; set; }
}