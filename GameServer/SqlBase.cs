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
            sql.ExecuteNonQuery("create table account(id INTEGER PRIMARY KEY AUTOINCREMENT, name VARCHAR[30] UNIQUE, password VARCHAR[255])");
            sql.ExecuteNonQuery("create table chars(id INTEGER PRIMARY KEY AUTOINCREMENT, accountId INTEGER, name VERCHAR[30] UNIQUE, class INTEGER, " +
                             "race INTEGER, locationId INTEGER, level INTEGER, exp INTEGER, map INTEGER, coordx INTEGER, coordy INTEGER, " +
                             "coordz INTEGER, CONSTRAINT FK_CharsAccountId FOREIGN KEY (accountId) REFERENCES account(id))");
            sql.ExecuteNonQuery("create table fraction(id INTEGER PRIMARY KEY AUTOINCREMENT, name VARCHAR[100] UNIQUE)");
            sql.ExecuteNonQuery("insert into fraction(name) Values ('Neutral'), ('Friendly')");

            sql.ExecuteNonQuery("create table skill(id INTEGER PRIMARY KEY AUTOINCREMENT, rank INTEGER, name VARCHAR[100], description TEXT," +
                                " cost FLOAT, targetFractionId INTEGER, CONSTRAINT FK_skillFractionTargetId FOREIGN KEY (targetFractionId) REFERENCES fraction(id))");
            sql.ExecuteNonQuery("create table skilllist(aid INTEGER PRIMARY KEY AUTOINCREMENT, skilllistid INTEGER, skillid INTEGER," +
                                " CONSTRAINT FK_SkillListSkillId FOREIGN KEY (skillid) REFERENCES skill(id))");
            sql.ExecuteNonQuery("create table stat(id INTEGER PRIMARY KEY AUTOINCREMENT, name VARCHAR[100], value INTEGER, description TEXT)");
            sql.ExecuteNonQuery("create table statlist(aid INTEGER PRIMARY KEY AUTOINCREMENT, statlistid INTEGER, statId INTEGER)");
            sql.ExecuteNonQuery("create table itemtype(id INTEGER PRIMARY KEY AUTOINCREMENT, type VARCHAR[100])");
            sql.ExecuteNonQuery("create table item(id INTEGER PRIMARY KEY AUTOINCREMENT, name VARCHAR[100], description TEXT, itemtypeid INTEGER," +
                                " statlistid INTEGER, maxsocket INTEGER, CONSTRAINT FK_ItemType FOREIGN KEY (itemtypeid) REFERENCES itemtype(id)," +
                                " CONSTRAINT FK_statlistid FOREIGN KEY (statlistid) REFERENCES statlist(statlistid))");
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
