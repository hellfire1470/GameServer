using System.Collections.Generic;
using GameData;
using FileManager;

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
        public SqlBase() : base("accounts.sqlite", (SQL sql) =>
        {
            sql.ExecuteNonQuery("create table account(id INTEGER PRIMARY KEY AUTOINCREMENT, name VARCHAR[30] UNIQUE, password VARCHAR[255])");
            sql.ExecuteNonQuery("create table chars(id INTEGER PRIMARY KEY AUTOINCREMENT, accountId INTEGER, name VERCHAR[30] UNIQUE, class INTEGER, " +
                             "race INTEGER, locationId INTEGER , level INTEGER, exp INTEGER, map INTEGER, coordx INTEGER, coordy INTEGER, " +
                             "coordz INTEGER, CONSTRAINT FK_CharsAccountId FOREIGN KEY (accountId) REFERENCES account(id))");
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
            Dictionary<int, Dictionary<string, string>> results = ExecuteQuery("select id, name, password from account where id = " + id);
            if (results.Count > 0)
            {
                return _SqlDataToAccount(results[0]);
            }
            return null;
        }

        public SqlAccountData GetAccount(string name)
        {
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
