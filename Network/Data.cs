using GameData;

namespace Network
{
    namespace Data
    {
        public class Account
        {
            public int Id;
            public string Username;
        }

        public class Character
        {
            public Character() { }
            public long Id;
            public string Name;
            public int Level;
            public RaceType Race;
            public ClassType Class;
            public long Experience;
            public Location Location;
            public FractionType Fraction;
        }
    }
}
