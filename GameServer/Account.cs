using System;

namespace GameServer
{
    class Account : Network.Data.Account
    {
        public bool Authentificated { get; private set; } = false;
        public bool InGame { get; private set; } = false;
        public Character Character { get; private set; } = null;

        public bool Login(string user, string password)
        {
            SqlAccountData sqlData = Globals.SqlBase.GetAccount(user);
            //todo:: hash password in database
            if (sqlData != null && user == sqlData.Username && password == sqlData.Password)
            {
                Authentificated = true;
                Username = sqlData.Username;
                Id = sqlData.Id;
            }
            return Authentificated;
        }

        public void Logout()
        {
            // todo:: do background server tasks to safely logoff player
            LeaveWorld();
            Authentificated = false;
        }

        private bool CanJoinWorld(Character character)
        {
            if (Authentificated && HasCharacter(character))
            {
                return true;
            }
            return false;
        }

        public bool JoinWorld(Character character)
        {
            if (CanJoinWorld(character))
            {
                Character = character;
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

        public Character[] GetCharacters()
        {
            if (Authentificated)
                return new Character[] { Character.GetCharacterById(0) };
            throw new Exception("Account not Authentificated");
        }

        private bool HasCharacter(Character character)
        {
            // todo: write function to check account has character
            return true;
        }
    }
}
