using System.Collections.Generic;
using GameData;
using Database;
using System.Security.Cryptography;
using System;

namespace GameServer
{

    public class SqlAccount : IAccount
    {
        private static Dictionary<int, IAccount> AccountsId = new Dictionary<int, IAccount>();
        private static Dictionary<string, IAccount> AccountsName = new Dictionary<string, IAccount>();

        private int _id;
        private string _username;
        private string _password;
        private int _maxCharacters;
        private string _gameKey;
        private bool _banned;

        public int Id { get { return _id; } private set { _id = value; } }
        public string Username { get { return _username; } private set { _username = value; } }
        public string Password { get { return _password; } private set { _password = value; } }
        public int MaxCharacters { get { return _maxCharacters; } private set { _maxCharacters = value; } }
        public string GameKey { get { return _gameKey; } private set { _gameKey = value; } }
        public bool Banned { get { return _banned; } private set { _banned = value; } }

        public static string HashPassword(string username, string password)
        {
            byte[] byteHash = SHA512.Create().ComputeHash(System.Text.Encoding.UTF8.GetBytes(username + password));
            return Convert.ToBase64String(byteHash);
        }

        public static bool Create(string name, string password)
        {
            //Filter illegal characters
            if (name.Trim() == "" || password.Trim() == "") return false;

            if (Global.SqlBase.ExecuteNonQuery("insert into account(name, password) values(@1, @2)", new string[] { name, HashPassword(name, password) }) == 1)
            {
                return true;
            }
            return false;
        }

        private static void Insert(IAccount account)
        {
            AccountsId[account.Id] = account;
            AccountsName[account.Username] = account;
        }

        public static IAccount Load(int id)
        {
            if (AccountsId.ContainsKey(id))
            {
                //return AccountsId[id];
            }

            Dictionary<int, Dictionary<string, string>> results = Global.SqlBase.ExecuteQuery("select id, name, password, gamekey, maxcharacters, banned from account where id = @1", new object[] { id });
            if (results.Count > 0)
            {
                IAccount iAccount = new SqlAccount
                {
                    Id = int.Parse(results[0]["id"]),
                    Username = results[0]["name"],
                    Password = results[0]["password"],
                    GameKey = results[0]["gamekey"],
                    MaxCharacters = int.Parse(results[0]["maxcharacters"]),
                    Banned = bool.Parse(results[0]["banned"])
                };
                Insert(iAccount);
                return iAccount;
            }
            return null;
        }

        public static IAccount Load(string name)
        {
            if (AccountsName.ContainsKey(name))
            {
                //return AccountsName[name];
            }

            Dictionary<int, Dictionary<string, string>> results = Global.SqlBase.ExecuteQuery("select id from account where name = @1", new string[] { name });
            if (results.Count > 0)
            {
                return Load(int.Parse(results[0]["id"]));
            }
            return null;
        }

        public static List<SqlCharacter> GetCharacters(int accountId)
        {
            List<int> characterIds = Global.SqlBase.GetCharacterIds(accountId);
            List<SqlCharacter> characters = new List<SqlCharacter>();
            foreach (int characterId in characterIds)
            {
                SqlCharacter character = SqlCharacter.Load(characterId);
                characters.Add(character);
            }
            return characters;
        }

        public static List<int> GetCharacterIds(int accountId)
        {
            return Global.SqlBase.GetCharacterIds(accountId);
        }
    }

    public class SqlCharacter
    {
        public static Dictionary<int, SqlCharacter> Characters = new Dictionary<int, SqlCharacter>();

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

        public SqlCharacter(int id, int accountId, string name, ClassType cl, RaceType race, int level, int exp, Location location, FractionType fraction, Dictionary<int, Dictionary<string, string>> meta)
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

        public static void Insert(SqlCharacter character)
        {
            Characters[character.Id] = character;
        }

        public static SqlCharacter Load(int id)
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
                SqlCharacter sqlCharacter = new SqlCharacter(int.Parse(character[0]["id"]), int.Parse(character[0]["accountid"]), character[0]["name"],
                                       (ClassType)int.Parse(character[0]["class"]), (RaceType)int.Parse(character[0]["race"]),
                                       int.Parse(character[0]["level"]), int.Parse(character[0]["exp"]),
                                                             new Location(MapManager.GetMap(int.Parse(locationData["mapid"])), int.Parse(locationData["x"]), int.Parse(locationData["y"]), int.Parse(locationData["z"])),
                                       (FractionType)int.Parse(character[0]["fraction"]), characterMeta);
                //Insert(sqlCharacter);
                return sqlCharacter;
            }
            return null;
        }

        public static SqlCharacter Load(string name)
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

            if (SqlAccount.GetCharacterIds(account.Id).Count >= account.MaxCharacters)
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
    }

    public class SqlSkill
    {
        public int Id { get; set; }
        public int Rank { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public float Cost { get; set; }
        // todo: do not use Fraction for targets. Use Enemy or Ally
        public FractionType Target { get; set; }
    }

    public class SqlStat
    {
        public StatType Type { get; set; }
        public float Value { get; set; }
    }

    public class SqlItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ItemType Type { get; set; }
        public List<SqlStat> Stats { get; set; }
        public int MaxSockets { get; set; }
        public ItemQuality Quality { get; set; }
    }

    public class SqlItemInstance
    {
        public int InstanceId { get; set; }
        public int ItemId { get; set; }
        public Dictionary<int, Dictionary<string, string>> Meta { get; set; }
    }

    public class SqlItemDrop
    {
        public SqlItem Item { get; set; }
        public float Chance { get; set; }
    }

    public class SqlDropList
    {
        public int Id { get; set; }
        public List<SqlItemDrop> Items { get; set; }
    }

    public class SqlEntity
    {
        public int Id { get; set; }
        public int TextureId { get; set; }
        public FractionType Fraction { get; set; }
        public int SkillListId { get; set; }
        public int DropListId { get; set; }
        public RaceType Race { get; set; }
        public string Name { get; set; }
        public EntityQuality Quality { get; set; }
        public int Life { get; set; }
        public int LifePL { get; set; }
        public int Level { get; set; }
        public int LevelRange { get; set; }
        public RessourceType RessourceType { get; set; }
        public float Ressource { get; set; }
        public float RessourcePL { get; set; }
        public int MoveSpeed { get; set; }
        public float AtkSpeed { get; set; }
        public float DPS { get; set; }
        public float DPSRange { get; set; }
        public float DPSPL { get; set; }
        public float DPSPLRange { get; set; }
        public float Exp { get; set; }
        public float ExpPL { get; set; }
        public Dictionary<string, string> Meta { get; set; }
    }

    public class SqlEntityInstance
    {
        public int InstanceId { get; set; }
        public int EntityId { get; set; }
        public Location Location { get; set; }
        public int Level { get; set; }
        public int Life { get; set; }
        public float Ressource { get; set; }
        public float MoveSpeed { get; set; }
        public float AtkSpeed { get; set; }
        public float DPS { get; set; }
        public float DPSRange { get; set; }
    }

    public class SqlBase : SQL
    {
        public SqlBase() : base(Settings.database_file, (SQL sql) =>
        {
            Logger.Log("Generating Database File ... ", true);
            string sqlCmd = "";

            //MAP
            sqlCmd += "create table map(id INTEGER PRIMARY KEY AUTOINCREMENT, name VARCHAR[100]);";
            sqlCmd += "insert into map(name) values('Erzbisho');";

            //LOCATION
            sqlCmd += "create table location(id INTEGER PRIMARY KEY AUTOINCREMENT, mapid INTEGER, x FLOAT, y FLOAT, z FLOAT," +
                " CONSTRAINT FK_locationmapid FOREIGN KEY (mapid) REFERENCES map(id));";

            //ACCOUNT
            sqlCmd += "create table account(id INTEGER PRIMARY KEY AUTOINCREMENT, name VARCHAR[30] UNIQUE, password VARCHAR[256], maxcharacters INTEGER DEFAULT 3, gamekey VARCHAR[16] UNIQUE, banned BOOLEAN DEFAULT 0, bannedAt TIMESTAMP, banreason VARCHAR[256]);";

            //CHARACTER
            sqlCmd += "create table character(id INTEGER PRIMARY KEY AUTOINCREMENT, accountid INTEGER, name VERCHAR[30] UNIQUE, class INTEGER," +
                " race INTEGER, level INTEGER, exp INTEGER, locationid INTEGER, fraction INTEGER," +
                " CONSTRAINT FK_CharsAccountId FOREIGN KEY (accountid) REFERENCES account(id)," +
                " CONSTRAINT FK_charslocationid FOREIGN KEY (locationid) REFERENCES location(id));";
            sqlCmd += "create table charactermeta(aid INTEGER PRIMARY KEY AUTOINCREMENT, characterid INTEGER, key VARCHAR[100], value TEXT," +
                " CONSTRAINT FK_charactermetacid FOREIGN KEY (characterid) REFERENCES character(id));";

            //SKILL
            sqlCmd += "create table skill(id INTEGER PRIMARY KEY AUTOINCREMENT, rank INTEGER, name VARCHAR[100], description TEXT," +
                " cost FLOAT, targetfraction INTEGER);";
            sqlCmd += "create table skilllist(aid INTEGER PRIMARY KEY AUTOINCREMENT, skilllistid INTEGER, skillid INTEGER," +
                " CONSTRAINT FK_SkillListSkillId FOREIGN KEY (skillid) REFERENCES skill(id));";

            //STATS
            sqlCmd += "create table statlist(aid INTEGER PRIMARY KEY AUTOINCREMENT, statlistid INTEGER, stat INTEGER, value FLOAT);";

            //ITEM
            sqlCmd += "create table item(id INTEGER PRIMARY KEY AUTOINCREMENT, name VARCHAR[100], description TEXT, itemtype INTEGER," +
                " statlistid INTEGER, maxsocket INTEGER, quality INTEGER," +
                " CONSTRAINT FK_statlistid FOREIGN KEY (statlistid) REFERENCES statlist(statlistid));";
            sqlCmd += "create table iteminstance(id INTEGER PRIMARY KEY AUTOINCREMENT, itemid INTEGER," +
                " CONSTRAINT FK_iteminstanceitemid FOREIGN KEY (itemid) REFERENCES item(id));";
            sqlCmd += "create table iteminstancemeta(aid INTEGER PRIMARY KEY AUTOINCREMENT, iteminstanceid INTEGER, key VARCHAR[100], value TEXT," +
                " CONSTRAINT FK_iteminstancemetaitemid FOREIGN KEY (iteminstanceid) REFERENCES iteminstance(id));";

            //DROPS
            sqlCmd += "create table droplist(aid INTEGER PRIMARY KEY AUTOINCREMENT, droplistid INTEGER, itemid INTEGER, chance FLOAT," +
                " CONSTRAINT FK_droplistitemid FOREIGN KEY (itemid) REFERENCES item(id));";

            //ENTITY
            sqlCmd += "create table entity(id INTEGER PRIMARY KEY AUTOINCREMENT, textureid INTEGER, fraction INTEGER, skilllistid INTEGER," +
                " droplistid INTEGER, race INTEGER, name VARCHAR[100], quality INTEGER, life INTEGER, lifepl FLOAT, level INTEGER, levelrange INTEGER," +
                " ressourcetype INTEGER, ressource INTEGER, movespeeed FLOAT, atkspeed FLOAT, dps FLOAT, dpspl FLOAT, dpsrange FLOAT, dpsplrange FLOAT, exppl INTEGER," +
                " CONSTRAINT FK_entityskillistid FOREIGN KEY (skilllistid) REFERENCES skilllist(id)," +
                " CONSTRAINT FK_entitydroplisid FOREIGN KEY (droplistid) REFERENCES droplist(id));";
            sqlCmd += "create table entitymeta(aid INTEGER PRIMARY KEY AUTOINCREMENT, entityid INTEGER, metakey VARCHAR[100], metavalue TEXT," +
                " CONSTRAINT FK_entitymetaentityid FOREIGN KEY (entityid) REFERENCES entity(id));";
            sqlCmd += "create table entityinstance(id INTEGER PRIMARY KEY AUTOINCREMENT, entityid INTEGER, locationid INTEGER, level INTEGER," +
                " life INTEGER, ressource FLOAT, movespeed FLOAT, atkspeed FLOAT, dps FLOAT, dpsrange FLOAT," +
                " CONSTRAINT FK_entityinstancenetityid FOREIGN KEY (entityid) REFERENCES entity(id)," +
                " CONSTRAINT FK_entityinstancelocationid FOREIGN KEY (locationid) REFERENCES location(id));";

            sql.ExecuteNonQuery(sqlCmd);
            Logger.Log("Done");
        })
        {
            Logger.Log("Checking Database ... ", true);
            int rows = ExecuteQuery("SELECT name FROM sqlite_master WHERE type='table';").Count;
            if (rows != 16)
            {
                Environment.Stop("Database corrupted! Please remove " + Settings.database_file + " file and restart the server");
                return;
            }
            Logger.Log("Done");
        }




        public List<int> GetCharacterIds(int accountId)
        {
            Dictionary<int, Dictionary<string, string>> characters = ExecuteQuery("SELECT id FROM character WHERE accountid = @1", new object[] { accountId });
            List<int> characterIds = new List<int>();
            foreach (Dictionary<string, string> character in characters.Values)
            {
                characterIds.Add(int.Parse(character["id"]));
            }
            return characterIds;
        }
    }


}
