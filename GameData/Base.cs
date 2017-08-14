namespace GameData
{
    public enum ErrorResult
    {
        UnknownError, Success, WrongData, InvalidGameKey, Banned, CharacterLimit, InvalidName, NameExists
    }
 
	public enum EntityQuality
	{
		// Important: do not change sorting order
		Normal, Rare, Elite, Boss, Legendary
	}
	public enum ItemQuality
	{
		// Important: do not change sorting order
		Useless, Normal, Magic, Rare, Epic, Legendary
	}
	public enum ItemType
	{
		// Important: do not change sorting order
		Helmet, Shoulder, Chest
	}

	public enum RessourceType
	{
		// Important: do not change sorting order
		None, Mana, Energy, Rage
	}

    public enum StatType
    {
        Strength, Vitality, Intelligence
    }

    public enum FractionType
    {
        // Important: do not change sorting order
        Neutral, A, B, C
    }

    public class Map
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Location
    {
        public Location(){}
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

    public enum CharacterAnimationType
    {
        Stop, Stand, Move, Run, Jump, Fall, Sit
    }

    public enum ClassType
    {
        // Important: do not change sorting order
		Mage, Warrior, Priest, Rouge
    }

    public enum RaceType
    {
		// Important: do not change sorting order
		Troll, Orc, Human, Goblin
    }
}
