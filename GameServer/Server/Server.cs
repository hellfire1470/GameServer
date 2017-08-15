using System.Net;
using Network;
using GameData.Network.Packages;
using System.Collections.Generic;

namespace GameServer.Server
{
    public class Server
    {
        private readonly Dictionary<IPEndPoint, User> _dictServerUser = new Dictionary<IPEndPoint, User>();
        private UdpServer _server;


        public Dictionary<IPEndPoint, User> ServerUser { get { return _dictServerUser; } }

        public void Start()
        {
            _server = new UdpServer(Global.Port);
            _server.DataReceived += OnServerReceive;
            _server.Start();

            Logger.Log("Listening on Port: " + Global.Port);
        }

        private void OnServerReceive(object sender, NetworkReceiveEventArgs e)
        {
            if (!_dictServerUser.ContainsKey(e.Sender))
            {
                User serverUser = new User(new Account(), e.Sender);
                serverUser.OnTimeout += () =>
                {
                    Logger.Log("[" + e.Sender.Address + "] was disconnected. Reason: Timeout");
                    serverUser.Account.Logout();
                    _dictServerUser.Remove(serverUser.IPEndPoint);
                };
                _dictServerUser.Add(e.Sender, serverUser);
            }
            _dictServerUser[e.Sender].RefreshActivity();
            Account account = _dictServerUser[e.Sender].Account;

            switch (NetworkHelperExtention.GetPackageType(e.Data))
            {
                #region "Login"
                case PackageType.Login:
                    LoginRequest request = NetworkHelper.Deserialize<LoginRequest>(e.Data);
                    GameData.ErrorResult loginResult = account.Login(request.Username, request.Password);
                    if (loginResult == GameData.ErrorResult.Success)
                    {
                        _server.SendBytes(e.Sender, NetworkHelper.Serialize(new LoginResponse { Success = true }));
                        Logger.Log("[" + e.Sender.Address + "] " + account.Username + " connected to Server");
                    }
                    else
                    {
                        _server.SendBytes(e.Sender, NetworkHelper.Serialize(new LoginResponse { Error = loginResult }));
                    }
                    break;
                #endregion
                #region "Logout"
                case PackageType.Logout:
                    LogoutRequest lr = NetworkHelper.Deserialize<LogoutRequest>(e.Data);
                    if (lr.Status == LogoutStatus.CharacterSelection)
                    {
                        account.LeaveWorld();
                        _server.SendBytes(e.Sender, NetworkHelper.Serialize(new LogoutResponse() { Success = true, Status = LogoutStatus.CharacterSelection }));
                    }
                    else if (lr.Status == LogoutStatus.TitleScreen)
                    {
                        account.Logout();
                        _server.SendBytes(e.Sender, NetworkHelper.Serialize(new LogoutResponse() { Success = true, Status = LogoutStatus.TitleScreen }));
                    }
                    break;
                #endregion
                #region "GetAccountCharacters"
                case PackageType.GetCharacters:
                    GetCharactersResponse gacr = new GetCharactersResponse() { Success = true, Characters = account.GetNetworkCharacters() };
                    _server.SendBytes(e.Sender, NetworkHelper.Serialize(gacr));
                    break;
                #endregion
                #region "SelectCharacter"
                case PackageType.SelectCharacter:
                    SelectCharacterRequest selectCharacterRequest = NetworkHelper.Deserialize<SelectCharacterRequest>(e.Data);
                    SelectCharacterResponse scr = new SelectCharacterResponse();
                    if (account.JoinWorld(selectCharacterRequest.CharacterId))
                    {
                        scr.Success = true;
                    }
                    _server.SendBytes(e.Sender, NetworkHelper.Serialize(scr));
                    break;
                #endregion

                #region "ChreateCharacter"
                case PackageType.CreateCharacter:
                    if (!account.Authentificated)
                    {
                        account.Logout();
                        _server.SendBytes(e.Sender, NetworkHelper.Serialize(new LogoutResponse { Success = true, Status = LogoutStatus.TitleScreen }));
                        break;
                    }
                    CreateCharacterRequest createCharacterRequest = NetworkHelper.Deserialize<CreateCharacterRequest>(e.Data);
                    GameData.Network.Character characterData = createCharacterRequest.CharacterData;
                    SQL.Character character = SQL.Character.Load(characterData.Name);
                    if (character != null)
                    {
                        _server.SendBytes(e.Sender, NetworkHelper.Serialize(new CreateCharacterResponse()
                        {
                            Success = false,
                            Error = GameData.ErrorResult.NameExists
                        }));
                        break;
                    }
                    GameData.ErrorResult creationResult = SQL.Character.Create(
                        _dictServerUser[e.Sender].Account, characterData.Name, characterData.Class,
                        characterData.Race, 1, 0, new GameData.Location(MapManager.GetMap(1), 0f, 1f, 2f), characterData.Fraction);
                    switch (creationResult)
                    {
                        case GameData.ErrorResult.Success:
                            _server.SendBytes(e.Sender, NetworkHelper.Serialize(new CreateCharacterResponse()
                            {
                                Success = true,
                                Error = GameData.ErrorResult.Success
                            }));
                            break;
                        default:
                            _server.SendBytes(e.Sender, NetworkHelper.Serialize(new CreateCharacterResponse()
                            {
                                Success = false,
                                Error = creationResult
                            }));
                            break;
                    }
                    break;
                #endregion
                default:
                    Logger.Error("Received Unknown Package");
                    break;
            }
        }
    }
}
