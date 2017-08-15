using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace GameServer.SQL
{
    public class Account : IAccount
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
                IAccount iAccount = new Account
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

        public static List<Character> GetCharacters(int accountId)
        {
            List<int> characterIds = Global.SqlBase.GetCharacterIds(accountId);
            List<Character> characters = new List<Character>();
            foreach (int characterId in characterIds)
            {
                Character character = Character.Load(characterId);
                characters.Add(character);
            }
            return characters;
        }

        public static List<int> GetCharacterIds(int accountId)
        {
            return Global.SqlBase.GetCharacterIds(accountId);
        }
    }
}
