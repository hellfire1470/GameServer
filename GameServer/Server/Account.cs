using System;
using System.Collections.Generic;
using GameData;
using GameData.Network;
using GameServer.Network;

namespace GameServer.Server
{
    public class Account : SQL.AccountData
    {
        public bool Authentificated { get; private set; } = false;
        public bool InGame { get; private set; } = false;
        public SQL.Character Character { get; private set; } = null;

        public Account(int id) : base(id) { }
        public Account(string name) : base(name) { }

        public ErrorResult Login(string password)
        {
            //todo:: hash password in database

            if (Password == HashPassword(Username, password))
            {

                if (GameKey == "")
                    return ErrorResult.InvalidGameKey;

                if (Banned)
                    return ErrorResult.Banned;

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
                Character = SQL.Character.Load(characterId);
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
                return SQL.AccountData.GetCharacterIds(Id);
            return new List<int>();
        }

        public Character[] GetNetworkCharacters()
        {
            List<int> characterIds = GetCharacterIdList();
            Character[] networkCharacters = new Character[characterIds.Count];
            for (int i = 0; i < characterIds.Count; i++)
            {
                networkCharacters[i] = Converter.ConvertCharacter(SQL.Character.Load(characterIds[i]));
            }
            return networkCharacters;
        }

        private bool HasCharacter(int characterId)
        {
            return GetCharacterIdList().Contains(characterId);
        }
    }
}
