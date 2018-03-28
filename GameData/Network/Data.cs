using GameData.Environment.Entity;
using GameData.Environment.Location;

namespace GameData.Network
{
    public class Account
    {
        public Account() { }
        public int Id;
        public string Username;
    }

    public struct Character
    {
        // public Character() { }
        public int Id;
        public SexType Sex;
        public string Name;
        public int Level;
        public RaceType Race;
        public ClassType Class;
        public long Experience;
        public Location Location;
        public FractionType Fraction;
        public int Aye;
        public int Nose;
        public int Mouth;
    }
}
