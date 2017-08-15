using System;

using GameServer.Network;
using GameData.Network.Packages;
using Network;
using GameData;

namespace GameServer.Server
{
    public static class PackageFunctions
    {
        private static Server _server;

        public static Package Login = new Package(PackageType.Login, LoginFunction);
        public static Package Logout = new Package(PackageType.Logout, LogoutFunction);
        public static Package CreateCharacter = new Package(PackageType.CreateCharacter, CreateCharacterFunction);
        public static Package GetCharacters = new Package(PackageType.GetCharacters, GetCharactersFunction);
        public static Package SelectCharacter = new Package(PackageType.SelectCharacter, SelectCharacterFunction);

        static PackageFunctions()
        {
            Logger.Log("Setting up Network functions ... ", true);
            PackageManager.RegisterPackage(Login);
            PackageManager.RegisterPackage(Logout);
            PackageManager.RegisterPackage(CreateCharacter);
            PackageManager.RegisterPackage(GetCharacters);
            PackageManager.RegisterPackage(SelectCharacter);
            Logger.Log("Done");
        }

        public static void SetServer(Server server)
        {
            _server = server;
        }

        public static void LoginFunction(Connection connection, NetworkReceiveEventArgs e)
        {
            LoginRequest request = NetworkHelper.Deserialize<LoginRequest>(e.Data);
            Account _account = new Account(request.Username);
            ErrorResult loginResult = _account.Login(request.Password);
            if (loginResult == ErrorResult.Success)
            {
                _server.ServerUser[e.Sender].SetAccount(_account);
                _server.SendBytes(e.Sender, NetworkHelper.Serialize(new LoginResponse { Success = true }));
                Logger.Log("[" + e.Sender.Address + "] " + _account.Username + " connected to Server");
            }
            else
            {
                _server.SendBytes(e.Sender, NetworkHelper.Serialize(new LoginResponse { Error = loginResult }));
            }
        }

        public static void LogoutFunction(Connection connection, NetworkReceiveEventArgs e)
        {
            connection.Account.Logout();
            _server.SendBytes(e.Sender, NetworkHelper.Serialize(new LogoutResponse { Success = true }));
        }

        public static void CreateCharacterFunction(Connection connection, NetworkReceiveEventArgs e)
        {
            // check if user is authentificated
            if (!connection.Account.Authentificated)
            {
                LogoutFunction(connection, null);
            }

            //getting the request
            CreateCharacterRequest createCharacterRequest = NetworkHelper.Deserialize<CreateCharacterRequest>(e.Data);

            // getting delivered data
            GameData.Network.Character characterData = createCharacterRequest.CharacterData;

            // checking character exists
            SQL.Character character = SQL.Character.Load(characterData.Name);
            if (character != null)
            {
                _server.SendBytes(e.Sender, NetworkHelper.Serialize(new CreateCharacterResponse()
                {
                    Success = false,
                    Error = ErrorResult.NameExists
                }));
                return;
            }

            // create character
            ErrorResult creationResult = SQL.Character.Create(connection.Account, characterData.Name, characterData.Class,
                characterData.Race, 1, 0, new Location(MapManager.GetMap(1), 0f, 1f, 2f), characterData.Fraction);

            // sending result to connection
            bool success = creationResult == ErrorResult.Success;
            _server.SendBytes(e.Sender, NetworkHelper.Serialize(new CreateCharacterResponse()
            {
                Success = success,
                Error = creationResult
            }));
        }

        public static void GetCharactersFunction(Connection connection, NetworkReceiveEventArgs e)
        {
            GetCharactersResponse gacr = new GetCharactersResponse { Success = true, Characters = connection.Account.GetNetworkCharacters() };
            _server.SendBytes(e.Sender, NetworkHelper.Serialize(gacr));
        }

        public static void SelectCharacterFunction(Connection connection, NetworkReceiveEventArgs e)
        {
            SelectCharacterRequest selectCharacterRequest = NetworkHelper.Deserialize<SelectCharacterRequest>(e.Data);
            SelectCharacterResponse scr = new SelectCharacterResponse();
            if (connection.Account.JoinWorld(selectCharacterRequest.CharacterId))
            {
                scr.Success = true;
            }
            _server.SendBytes(e.Sender, NetworkHelper.Serialize(scr));
        }


    }
}
