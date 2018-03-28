using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using GameData.Network;

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


        protected virtual ResultType Create(string name, string password)
        {
            if (name.Length == 0) return ResultType.NameEmpty;
            if (password.Trim().Length == 0) return ResultType.PasswordEmpty;

            if (Global.SqlBase.ExecuteNonQuery("insert into account(name, password) values(@1, @2)", new string[] { name, HashPassword(name, password) }) == 1)
            {
                return ResultType.Success;
            }
            //todo: remove test only
            SetGameKey("123");
            // endremove
            return ResultType.UnknownError;
        }

        public void SetGameKey(string gameKey)
        {
            //todo: check for valid game keys
            if (Global.SqlBase.ExecuteNonQuery("update account set gamekey = @1 where id = @2", new string[] { gameKey, Id.ToString() }) == 1)
            {
                Logger.Error("gamekey updated!");
            }
            Logger.Error("failed update gamekey");
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

        public static List<CharacterData> GetCharacters(int accountId)
        {
            List<int> characterIds = CharacterData.GetCharacterIds(accountId);
            List<CharacterData> characters = new List<CharacterData>();
            foreach (int characterId in characterIds)
            {
                CharacterData character = CharacterData.Load(characterId);
                characters.Add(character);
            }
            return characters;
        }

        public static List<int> GetCharacterIds(int accountId)
        {
            return CharacterData.GetCharacterIds(accountId);
        }
    }
}
