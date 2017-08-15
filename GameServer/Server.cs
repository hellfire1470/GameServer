using System;
using Network;
using Network.Packages;
using System.Collections.Generic;
using System.Net;
using FileManager;
using System.Threading;

namespace GameServer
{
    public class ServerUser
    {
        public static double TimeoutTimeInMinutes = 1;

        public IPEndPoint IPEndPoint { get; private set; }
        public Account Account { get; private set; }
        public DateTime LastActivity { get; private set; }

        public event Action OnTimeout;

        private Thread TimeoutThread;


        public ServerUser(Account account, IPEndPoint ipendpoint)
        {
            Account = account;
            IPEndPoint = ipendpoint;
            RefreshActivity();

            TimeoutThread = new Thread(TimeoutThreadFunction);
            TimeoutThread.Start();
        }

        public void RefreshActivity()
        {
            LastActivity = DateTime.Now;
        }

        private void TimeoutThreadFunction()
        {
            while (LastActivity.AddMinutes(TimeoutTimeInMinutes) > DateTime.Now)
            {
                // Check once a minute
                Thread.Sleep(60000);
            }

            OnTimeout?.Invoke();
        }
    }

    static class Settings
    {
        public const string configuration_file = "config.ini";
        public const string database_file = "data.sqlite";
        public static string log_dir = "logs/";
        public static string log_file = DateTime.Now.ToString("yy.MM.dd_HHmmss") + ".log";
        public const double timeout_in_minutes = 15d;
    }

    static class Global
    {
        public static Config ConfigurationFile;

        public static SqlBase SqlBase;

        public static string Name = "Example Name";
        public static int Port = 5090;
        public static List<long> Admins = new List<long>();
    }

    static class Environment
    {
        public static void Stop(string reason = "")
        {
            //todo: save all data
            if (reason != "")
            {
                Logger.Error("Server stopped! Reason: " + reason);
            }
            Logger.Log("Press Enter to quit");
            Console.ReadLine();
            System.Environment.Exit(-1);
        }
    }

    class MainClass
    {
        public static Dictionary<IPEndPoint, ServerUser> _dictServerUser = new Dictionary<IPEndPoint, ServerUser>();
        public static UdpServer _server;

        public static void Main(string[] args)
        {
            try
            {
                StartupServer(args);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
            while (true)
            {
                string input = Console.ReadLine();
                try
                {
                    CommandListener.HandleCommand(input);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.Message);
                }
            }
            // Environment.Exit(0);
        }

        public static void StartupServer(string[] args)
        {
            DateTime startTime = DateTime.Now;

			Logger.LogDir = Settings.log_dir;
            Logger.LogFile = Settings.log_file;
            Logger.LogToFile = true;

			// Configuration File
			LoadConfigurationFile();

            // Database
            LoadDatabaseFile();


            ApplyStartupArgs(args);

            SetupCommands();

            Logger.Log("Listening on Port: " + Global.Port);
            _server = new UdpServer(Global.Port);
            _server.DataReceived += OnServerReceive;
            _server.Start();
            Logger.Log("Server started");
            Logger.Log("Startup took " + DateTime.Now.Subtract(startTime).TotalSeconds + " seconds");
            Logger.Log("Server Name: " + Global.Name);
        }

        public static void LoadDatabaseFile()
        {
            Global.SqlBase = new SqlBase();
            Logger.Log("Loading Maps ... ", true);
            MapManager.LoadMaps(Global.SqlBase);
            Logger.Log("Done");


        }

        public static void SetupCommands()
        {
            Logger.Log("Setup commands ... ", true);
            CommandListener.AddCommand(CommandManager.Quit);
            CommandListener.AddCommand(CommandManager.ChangeName);
            CommandListener.AddCommand(CommandManager.Online);
            CommandListener.AddCommand(CommandManager.ChangePort);
            CommandListener.AddCommand(CommandManager.Help);
            CommandListener.AddCommand(CommandManager.Show);
            CommandListener.AddCommand(CommandManager.Account);
            CommandListener.AddCommand(CommandManager.Character);
            Logger.Log("Done");
        }

        public static void LoadConfigurationFile()
        {
            Global.ConfigurationFile = new Config(Settings.configuration_file);

            foreach (Config.KeyValue keyvalue in Global.ConfigurationFile.GetConfigs())
            {
                switch (keyvalue.Key.ToLower())
                {
                    case "name":
                        Global.Name = keyvalue.Value;
                        break;
                    case "port":
                        int.TryParse(keyvalue.Value, out Global.Port);
                        break;
                    case "admins":
                        foreach (string id in keyvalue.Value.Split(','))
                        {
                            if (long.TryParse(id, out long Id))
                            {
                                Global.Admins.Add(Id);
                            }
                        }
                        break;
                    case "log_dir":
                        Settings.log_dir = keyvalue.Value;
                        Logger.LogDir = keyvalue.Value;
                        break;
                    case "log_file":
                        Settings.log_file = keyvalue.Value;
                        Logger.LogFile = keyvalue.Value;
                        break;
                    case "log_to_file":
                        Logger.LogToFile = bool.Parse(keyvalue.Value);
                        break;
                }
            }
        }

        public static void ApplyStartupArgs(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-p")
                {
                    int.TryParse(args[i + 1], out Global.Port);
                }
            }
            Account.TimeoutInMinutes = Settings.timeout_in_minutes;
        }

        public static void OnServerReceive(object sender, NetworkReceiveEventArgs e)
        {
            if (!_dictServerUser.ContainsKey(e.Sender))
            {
                ServerUser serverUser = new ServerUser(new Account(), e.Sender);
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

            switch (NetworkHelper.GetPackageType(e.Data))
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
                case PackageType.GetAccountCharacters:
                    GetAccountCharactersResponse gacr = new GetAccountCharactersResponse() { Success = true, Characters = account.GetNetworkCharacters() };
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
                    Network.Data.Character characterData = createCharacterRequest.CharacterData;
                    SqlCharacter character = SqlCharacter.Load(characterData.Name);
                    if (character != null)
                    {
                        _server.SendBytes(e.Sender, NetworkHelper.Serialize(new CreateCharacterResponse()
                        {
                            Success = false,
                            Error = GameData.ErrorResult.NameExists
                        }));
                        break;
                    }
                    GameData.ErrorResult creationResult = SqlCharacter.Create(
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
