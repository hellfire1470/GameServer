using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace GameServer.SQL
{
    public class AccountData
    {
        public int Id { get; protected set; }
        public string Username { get; protected set; }
        public string Password { get; protected set; }
        public int MaxCharacters { get; protected set; }
        public string GameKey { get; protected set; }
        public bool Banned { get; protected set; }

        public AccountData(int id) { Load(id); }
        public AccountData(string name) { Load(name); }

        public static string HashPassword(string username, string password)
        {
            byte[] byteHash = SHA512.Create().ComputeHash(System.Text.Encoding.UTF8.GetBytes(username + password));
            return Convert.ToBase64String(byteHash);
        }

        public static bool Create(string name, string password)
        {
            //Todo: Filter illegal characters
            if (name.Trim() == "" || password.Trim() == "") return false;

            if (Global.SqlBase.ExecuteNonQuery("insert into account(name, password) values(@1, @2)", new string[] { name, HashPassword(name, password) }) == 1)
            {
                return true;
            }
            return false;
        }

        public void Load(int id)
        {

            Dictionary<int, Dictionary<string, string>> results = Global.SqlBase.ExecuteQuery("select id, name, password, gamekey, maxcharacters, banned from account where id = @1", new object[] { id });
            if (results.Count > 0)
            {
                Id = int.Parse(results[0]["id"]);
                Username = results[0]["name"];
                Password = results[0]["password"];
                GameKey = results[0]["gamekey"];
                MaxCharacters = int.Parse(results[0]["maxcharacters"]);
                Banned = bool.Parse(results[0]["banned"]);
            }
        }

        public void Load(string name)
        {
            Dictionary<int, Dictionary<string, string>> results = Global.SqlBase.ExecuteQuery("select id from account where name = @1", new string[] { name });
            if (results.Count > 0)
            {
                Load(int.Parse(results[0]["id"]));
            }
        }

        public static List<Character> GetCharacters(int accountId)
        {
            List<int> characterIds = Character.GetCharacterIds(accountId);
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
            return Character.GetCharacterIds(accountId);
        }
    }
}
