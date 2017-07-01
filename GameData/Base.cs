using System;
namespace GameData
{
    public class Location{
        public int Map;
        public string Name;
        public float CoordX;
        public float CoordY;
        public float CoordZ;
    }

    public enum Class{
        Mage, Warrior, Priest, Rouge
    }

    public enum Race{
        Troll, Orc, Human, Goblin
    }
}
