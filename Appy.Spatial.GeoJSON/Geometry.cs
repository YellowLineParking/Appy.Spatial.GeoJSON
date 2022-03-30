namespace Appy.Spatial.GeoJSON
{
    public abstract class Geometry
    {
        public string Type { get; set; }
        public Crs Crs { get; set; }
    }

    public abstract class Geometry<T> : Geometry
    {
        protected Geometry(string type)
        {
            Type = type;
        }

        protected Geometry(string type, T coordinates)
        {
            Type = type;
            Coordinates = coordinates;
        }
        
        public T Coordinates { get; set; }
    }

    

 
}