using System;
using System.Threading;
using System.Collections.Generic;
using GameData;

namespace GameServer
{

    public interface IAccount
    {
        int Id { get; }
        string Username { get; }
        string Password { get; }
        int MaxCharacters { get; }
        string GameKey { get; }
        bool Banned { get; }
    }


    public class Account : IAccount
    {
        // This is overwritten in Server->ApplyStartupArgs function
        public static double TimeoutInMinutes = 15;

        public bool Authentificated { get; private set; } = false;
        public bool InGame { get; private set; } = false;
        public SqlCharacter Character { get; private set; } = null;

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

        //  public SqlAccount SqlAccount { get; private set; } = null;

        public ErrorResult Login(string user, string password)
        {
            IAccount iAccount = SqlAccount.Load(user);
            //todo:: hash password in database

            if (iAccount.Username == user && iAccount.Password == SqlAccount.HashPassword(user, password))
            {

                if (iAccount.GameKey == "")
                    return ErrorResult.InvalidGameKey;

                if (iAccount.Banned)
                    return ErrorResult.Banned;

                Id = iAccount.Id;
                Username = iAccount.Username;
                Password = iAccount.Password;
                GameKey = iAccount.GameKey;
                MaxCharacters = iAccount.MaxCharacters;
                Banned = iAccount.Banned;
                Authentificated = true;
                return ErrorResult.Success;
            }
            else
                return ErrorResult.WrongData;
        }

        public void Logout()
        {
            // todo:: do background server tasks to safely logoff player
            LeaveWorld();
            Id = 0;
            Username = null;
            Password = null;
            Authentificated = false;
        }

        private bool CanJoinWorld(int characterId)
        {
            if (Authentificated && HasCharacter(characterId))
            {
                return true;
            }
            return false;
        }

        public bool JoinWorld(int characterId)
        {
            if (CanJoinWorld(characterId))
            {
                Character = SqlCharacter.Load(characterId);
                InGame = true;
                return true;
            }
            return false;
        }

        public void LeaveWorld()
        {
            InGame = false;
            Character = null;
        }

        public List<int> GetCharacterIdList()
        {
            if (Authentificated)
                return SqlAccount.GetCharacterIds(Id);
            return new List<int>();
        }

        public Network.Data.Character[] GetNetworkCharacters()
        {
            List<int> characterIds = GetCharacterIdList();
            Network.Data.Character[] networkCharacters = new Network.Data.Character[characterIds.Count];
            for (int i = 0; i < characterIds.Count; i++)
            {
                networkCharacters[i] = NetworkConverter.NetworkCharacter(SqlCharacter.Load(characterIds[i]));
            }
            return networkCharacters;
        }

        private bool HasCharacter(int characterId)
        {
            return GetCharacterIdList().Contains(characterId);
        }
    }
}
