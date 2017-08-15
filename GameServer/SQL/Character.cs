using System;
using GameData;
using System.Collections.Generic;

namespace GameServer.SQL
{
    public class Character
    {
        public static Dictionary<int, Character> Characters = new Dictionary<int, Character>();

        public int Id { get; set; }
        public int AccountId { get; set; }
        public string Name { get; set; }
        public ClassType Class { get; set; }
        public RaceType Race { get; set; }
        public int Level { get; set; }
        public int Exp { get; set; }
        public Location Location { get; set; }
        public FractionType Fraction { get; set; }
        public Dictionary<int, Dictionary<string, string>> Meta { get; set; }

        public Character(int id, int accountId, string name, ClassType cl, RaceType race, int level, int exp, Location location, FractionType fraction, Dictionary<int, Dictionary<string, string>> meta)
        {
            Id = id;
            AccountId = accountId;
            Name = name;
            Class = cl;
            Race = race;
            Level = level;
            Exp = exp;
            Location = location;
            Fraction = fraction;
            Meta = meta;
        }

        public static void Insert(Character character)
        {
            Characters[character.Id] = character;
        }

        public static Character Load(int id)
        {
            if (Characters.ContainsKey(id))
            {
                //return Characters[id];
            }

            Dictionary<int, Dictionary<string, string>> character = Global.SqlBase.ExecuteQuery("SELECT id, accountid, class, level, fraction, exp, name, race, locationid FROM character WHERE id = @1", new object[] { id });

            if (character.Count == 1)
            {
                Dictionary<string, string> locationData = Global.SqlBase.ExecuteQuery("SELECT mapid, x, y, z, name FROM location JOIN map ON map.id = mapid WHERE location.id = @1", new object[] { character[0]["locationid"] })[0];
                Dictionary<int, Dictionary<string, string>> characterMeta = Global.SqlBase.ExecuteQuery("SELECT key, value FROM charactermeta WHERE characterid = @1", new object[] { character[0]["id"] });
                if (locationData == null)
                {
                    return null;
                }
                Character sqlCharacter = new Character(int.Parse(character[0]["id"]), int.Parse(character[0]["accountid"]), character[0]["name"],
                                       (ClassType)int.Parse(character[0]["class"]), (RaceType)int.Parse(character[0]["race"]),
                                       int.Parse(character[0]["level"]), int.Parse(character[0]["exp"]),
                                                             new Location(MapManager.GetMap(int.Parse(locationData["mapid"])), int.Parse(locationData["x"]), int.Parse(locationData["y"]), int.Parse(locationData["z"])),
                                       (FractionType)int.Parse(character[0]["fraction"]), characterMeta);
                //Insert(sqlCharacter);
                return sqlCharacter;
            }
            return null;
        }

        public static Character Load(string name)
        {
            // todo: sql injection protection
            Dictionary<int, Dictionary<string, string>> characters = Global.SqlBase.ExecuteQuery("SELECT id FROM character WHERE name = @1", new string[] { name });
            if (characters.Count == 1)
            {
                return Load(int.Parse(characters[0]["id"]));
            }
            return null;
        }

        public static ErrorResult Create(IAccount account, string name, ClassType cl, RaceType race, int level, int exp, Location location, FractionType fraction)
        {
            if (name.Trim() == "") return ErrorResult.InvalidName;

            if (Account.GetCharacterIds(account.Id).Count >= account.MaxCharacters)
            {
                return ErrorResult.CharacterLimit;
            }
            Global.SqlBase.ExecuteQuery("INSERT INTO location(mapid, x, y, z) VALUES (@1, @2, @3, @4)",
                                        new object[] { location.Map.Id, location.CoordX, location.CoordY, location.CoordZ });
            long locationId = Global.SqlBase.LastInsertId();
            return Global.SqlBase.ExecuteNonQuery(
                "INSERT INTO character(accountid, name, class, race, level, exp, locationid, fraction) " +
                "VALUES (@1, @2, @3, @4, @5, @6, @7, @8)", new object[] { account.Id, name, cl, race, level, exp, locationId, fraction }
            ) == 1 ? ErrorResult.Success : ErrorResult.UnknownError;
        }

        public static List<int> GetCharacterIds(int accountId)
        {
            Dictionary<int, Dictionary<string, string>> characters = Global.SqlBase.ExecuteQuery("SELECT id FROM character WHERE accountid = @1", new object[] { accountId });
            List<int> characterIds = new List<int>();
            foreach (Dictionary<string, string> character in characters.Values)
            {
                characterIds.Add(int.Parse(character["id"]));
            }
            return characterIds;
        }
    }
}
