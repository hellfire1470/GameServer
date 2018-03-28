using GameData;
using System.Collections.Generic;
using GameData.Environment.Entity;
using GameData.Environment.Location;
using GameData.Network;
using System.Linq.Expressions;

namespace GameServer.SQL
{

    public interface ICharacterData
    {
        int Id { get; }
        int AccountId { get; }
        string Name { get; }
        ClassType Class { get; }
        RaceType Race { get; }
        int Level { get; }
        int Exp { get; }
        Location Location { get; }
        FractionType Fraction { get; }
        Dictionary<int, Dictionary<string, string>> Meta { get; }

    }

    public class CharacterData : ICharacterData
    {

        public int Id { get; protected set; }
        public int AccountId { get; protected set; }
        public string Name { get; protected set; }
        public ClassType Class { get; protected set; }
        public RaceType Race { get; private set; }
        public int Level { get; private set; }
        public int Exp { get; private set; }
        public Location Location { get; private set; }
        public FractionType Fraction { get; private set; }
        public Dictionary<int, Dictionary<string, string>> Meta { get; private set; }

        public static CharacterData Load(int id)
        {
            Dictionary<int, Dictionary<string, string>> character = Global.SqlBase.ExecuteQuery("SELECT id, accountid, class, level, fraction, exp, name, race, locationid FROM character WHERE id = @1", new object[] { id });

            if (character.Count == 1)
            {
                Dictionary<string, string> locationData = Global.SqlBase.ExecuteQuery("SELECT mapid, x, y, z, name FROM location JOIN map ON map.id = mapid WHERE location.id = @1", new object[] { character[0]["locationid"] })[0];
                Dictionary<int, Dictionary<string, string>> characterMeta = Global.SqlBase.ExecuteQuery("SELECT key, value FROM charactermeta WHERE characterid = @1", new object[] { character[0]["id"] });
                if (locationData == null)
                {
                    return null;
                }
                CharacterData iCharacter = new CharacterData
                {
                    Id = int.Parse(character[0]["id"]),
                    AccountId = int.Parse(character[0]["accountid"]),
                    Name = character[0]["name"],
                    Class = (ClassType)int.Parse(character[0]["class"]),
                    Race = (RaceType)int.Parse(character[0]["race"]),
                    Level = int.Parse(character[0]["level"]),
                    Exp = int.Parse(character[0]["exp"]),
                    Location = new Location(MapManager.GetMap(int.Parse(locationData["mapid"])), int.Parse(locationData["x"]), int.Parse(locationData["y"]), int.Parse(locationData["z"])),
                    Fraction = (FractionType)int.Parse(character[0]["fraction"]),
                    Meta = characterMeta
                };
                return iCharacter;
            }
            return null;
        }

        public static CharacterData Load(string name)
        {
            // todo: sql injection protection
            Dictionary<int, Dictionary<string, string>> characters = Global.SqlBase.ExecuteQuery("SELECT id FROM character WHERE name = @1", new string[] { name });
            if (characters.Count == 1)
            {
                return Load(int.Parse(characters[0]["id"]));
            }
            return null;
        }

        public virtual ResultType Create(AccountData account)
        {
            if (AccountData.GetCharacterIds(account.Id).Count >= account.MaxCharacters)
            {
                return ResultType.CharacterLimit;
            }

            Location = new Location(MapManager.GetMap(1), 0f, 0f, 0f);
            Fraction = FractionType.A;

            // todo: create Transaction
            Global.SqlBase.ExecuteQuery("INSERT INTO location(mapid, x, y, z) VALUES (@1, @2, @3, @4)",
                                        new object[] { Location.Map.Id, Location.CoordX, Location.CoordY, Location.CoordZ });
            long locationId = Global.SqlBase.LastInsertId();
            return Global.SqlBase.ExecuteNonQuery(
                "INSERT OR IGNORE INTO character(accountid, name, class, race, level, exp, locationid, fraction) " +
                "VALUES (@1, @2, @3, @4, @5, @6, @7, @8)", new object[] { account.Id, Name, Class, Race, 1, 0, locationId, Fraction }
            ) == 1 ? ResultType.Success : ResultType.UnknownError;
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
