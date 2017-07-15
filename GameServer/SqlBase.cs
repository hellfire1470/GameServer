using System.Collections.Generic;
using GameData;
using Database;

namespace GameServer
{
    public class SqlAccountData
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class SqlCharacterData
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public string Name { get; set; }
        public Class Class { get; set; }
        public Race Race { get; set; }
        public Location Location { get; set; }
        public int Level { get; set; }
        public int Exp { get; set; }
        public int Map { get; set; }
        public int CoordX { get; set; }
        public int CoordY { get; set; }
        public int CoordZ { get; set; }
    }

    public class SqlBase : SQL
    {
        public SqlBase() : base("data.sqlite", (SQL sql) =>
        {
            System.Console.WriteLine("Generating Database File ... ");
            string sqlCmd = "";

            //MAP
            sqlCmd += "create table map(id INTEGER PRIMARY KEY AUTOINCREMENT, name VARCHAR[100]);";

            //LOCATION
            sqlCmd += "create table location(id INTEGER PRIMARY KEY AUTOINCREMENT, mapid INTEGER, x FLOAT, y FLOAT, z FLOAT," +
                " CONSTRAINT FK_locationmapid FOREIGN KEY (mapid) REFERENCES map(id));";

            //FRACTION
            sqlCmd += "create table fraction(id INTEGER PRIMARY KEY AUTOINCREMENT, name VARCHAR[100] UNIQUE);";
            sqlCmd += "insert into fraction(name) Values ('Neutral'), ('Friendly');";

            //ACCOUNT
            sqlCmd += "create table account(id INTEGER PRIMARY KEY AUTOINCREMENT, name VARCHAR[30] UNIQUE, password VARCHAR[255]);";

            //CHARACTER
            sqlCmd += "create table character(id INTEGER PRIMARY KEY AUTOINCREMENT, accountid INTEGER, name VERCHAR[30] UNIQUE, class INTEGER," +
                " race INTEGER, level INTEGER, exp INTEGER, locationid INTEGER, fractionid INTEGER," +
                " CONSTRAINT FK_CharsAccountId FOREIGN KEY (accountid) REFERENCES account(id)," +
                " CONSTRAINT FK_charslocationid FOREIGN KEY (locationid) REFERENCES location(id)," +
                " CONSTRAINT FK_characterfractionid FOREIGN KEY (fractionid) REFERENCES fraction(id));";
            sqlCmd += "create table charactermeta(aid INTEGER PRIMARY KEY AUTOINCREMENT, characterid INTEGER, key VARCHAR[100], value TEXT," +
                " CONTRAINT FK_charactermetacharacterid FOREIGN KEY (characterid) REFERENCES character(id));";

            //SKILL
            sqlCmd += "create table skill(id INTEGER PRIMARY KEY AUTOINCREMENT, rank INTEGER, name VARCHAR[100], description TEXT," +
                " cost FLOAT, targetFractionId INTEGER," +
                " CONSTRAINT FK_skillFractionTargetId FOREIGN KEY (targetFractionId) REFERENCES fraction(id));";
            sqlCmd += "create table skilllist(aid INTEGER PRIMARY KEY AUTOINCREMENT, skilllistid INTEGER, skillid INTEGER," +
                " CONSTRAINT FK_SkillListSkillId FOREIGN KEY (skillid) REFERENCES skill(id));";

            //STATS
            sqlCmd += "create table stat(id INTEGER PRIMARY KEY AUTOINCREMENT, name VARCHAR[100], value INTEGER, description TEXT);";
            sqlCmd += "create table statlist(aid INTEGER PRIMARY KEY AUTOINCREMENT, statlistid INTEGER, statId INTEGER);";

            //ITEM
            sqlCmd += "create table itemquality(id INTEGER PRIMARY KEY AUTOINCREMENT, quality VARCHAR[100]);";
            sqlCmd += "create table itemtype(id INTEGER PRIMARY KEY AUTOINCREMENT, type VARCHAR[100]);";
            sqlCmd += "create table item(id INTEGER PRIMARY KEY AUTOINCREMENT, name VARCHAR[100], description TEXT, itemtypeid INTEGER," +
                " statlistid INTEGER, maxsocket INTEGER, qualtyid INTEGER," +
                " CONSTRAINT FK_ItemType FOREIGN KEY (itemtypeid) REFERENCES itemtype(id)," +
                " CONSTRAINT FK_statlistid FOREIGN KEY (statlistid) REFERENCES statlist(statlistid)" +
                " CONSTRAINT FK_itemqualityid FOREIGN KEY (qualityid) REFERENCES itemquality(id));";
            sqlCmd += "create table iteminstance(id INTEGER PRIMARY KEY AUTOINCREMENT, itemid INTEGER, CONSTRAINT FK_iteminstanceitemid" +
                " FOREIGN KEY (itemid) REFERENCES item(id));";
            sqlCmd += "create table iteminstancemeta(aid INTEGER PRIMARY KEY AUTOINCREMENT, iteminstanceid INTEGER, key VARCHAR[100], value TEXT," +
                " CONSTRAINT FK_iteminstancemetaitemid FOREIGN KEY (iteminstanceid) REFERENCES iteminstance(id));";

            //DROPS
            sqlCmd += "create table droplist(aid INTEGER PRIMARY KEY AUTOINCREMENT, itemid INTEGER, chance FLOAT," +
                " CONSTRAINT FK_droplistitemid FOREIGN KEY (itemid) REFERENCES item(id));";

            // ENTITY
            sqlCmd += "create table entityquality(id INTEGER PRIMARY KEY AUTOINCREMENT, quality VARCHAR[100];";
            sqlCmd += "create table entity(id INTEGER PRIMARY KEY AUTOINCREMENT, textureid INTEGER, fractionid INTEGER, skilllistid INTEGER," +
                " droplistid INTEGER, raceid INTEGER, name VARCHAR[100], qualityid INTEGER, lifepl FLOAT, level INTEGER, levelrange INTEGER," +
                " ressourceid INTEGER, maxressource INTEGER, maxmovespeeed FLOAT, maxatkspeed FLOAT, damage FLOAT, damagerange FLOAT, exppl INTEGER," +
                " CONSTRAINT FK_entityfractionid FOREIGN KEY (fractionid) REFERENCES fraction(id)," +
                " CONSTRAINT FK_entityskillistid FOREIGN KEY (skilllistid) REFERENCES skilllist(id)," +
                " CONSTRAINT FK_entitydroplisid FOREIGN KEY (droplistid) REFERENCES droplist(id)," +
                " CONSTRAINT FK_entityqualityid FOREIGN KEY (qualityid) REFERENCES entityquality(id));";
            sqlCmd += "create table entitymeta(aid INTEGER PRIMARY KEY AUTOINCREMENT, entityid INTEGER, metakey VARCHAR[100], metavalue TEXT," +
                " CONSTRAINT FK_entitymetaentityid FOREIGN KEY (entityid) REFERENCES entity(id));";
            sqlCmd += "create table entityinstance(id INTEGER PRIMARY KEY AUTOINCREMENT, entityid INTEGER, locationid INTEGER, level INTEGER," +
                " life INTEGER, ressource FLOAT, movespeed FLOAT, atkspeed FLOAT, damage FLOAT, damagerange FLOAT," +
                " CONSTRAINT FK_entityinstancenetityid FOREIGN KEY (entityid) REFERENCES entity(id)," +
                " CONSTRAINT FK_entityinstancelocationid FOREIGN KEY (locationid) REFERENCES location(id));";

            sql.ExecuteNonQuery(sqlCmd);
            System.Console.WriteLine("Generation done ... ");
        })
        {

        }

        public bool CreateAccount(string user, string password)
        {
            //todo: sql injection protection
            if (ExecuteNonQuery("insert into account(name, password) values('" + user + "','" + password + "')") == 1)
            {
                return true;
            }
            return false;
        }

        public SqlAccountData GetAccount(int id)
        {
            //todo: sql injection protection
            Dictionary<int, Dictionary<string, string>> results = ExecuteQuery("select id, name, password from account where id = " + id);
            if (results.Count > 0)
            {
                return _SqlDataToAccount(results[0]);
            }
            return null;
        }

        public SqlAccountData GetAccount(string name)
        {
            //todo: sql injection protection
            Dictionary<int, Dictionary<string, string>> results = ExecuteQuery("select id, name, password from account where name = '" + name + "'");
            if (results.Count > 0)
            {
                return _SqlDataToAccount(results[0]);
            }
            return null;
        }

        private SqlAccountData _SqlDataToAccount(Dictionary<string, string> sqlData)
        {
            return new SqlAccountData()
            {
                Id = int.Parse(sqlData["id"]),
                Username = sqlData["name"],
                Password = sqlData["password"]
            };
        }
    }
}
