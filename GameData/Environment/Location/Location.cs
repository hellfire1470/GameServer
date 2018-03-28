using System;
namespace GameData.Environment.Location
{
    public class Location
    {
        public Location() { }
        public Map Map { get; set; }
        public float CoordX { get; set; }
        public float CoordY { get; set; }
        public float CoordZ { get; set; }

        public Location(Map map, float x, float y, float z)
        {
            Map = map;
            CoordX = x;
            CoordY = y;
            CoordZ = z;
        }
    }

}
