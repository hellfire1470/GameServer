using System;
using Network;
using Network.Packages;
using System.Collections.Generic;
using System.Net;
using FileManager;

namespace GameServer
{
    static class Globals{
        public static string Name = "Example Name";
        public static int Port = 5090;
        public static List<long> Admins = new List<long>();
    }

    class MainClass
    {
        public static Dictionary<IPEndPoint, Account> _accounts = new Dictionary<IPEndPoint, Account>();
        public static UdpServer _server;

        public static void Main(string[] args)
        {
            Console.WriteLine("Starting Server ...");
            LoadConfigurationFile();
            ApplyStartupArgs(args);

            Console.WriteLine("Listening on Port: " + Globals.Port);
            _server = new UdpServer(Globals.Port);
            _server.DataReceived += OnServerReceive;
            _server.Start();
            Console.WriteLine("Server started");
            Console.WriteLine("Server Name: " + Globals.Name);
            while (true)
            {
                string input = Console.ReadLine().ToLower();
                if (input == "quit")
                {
                    //todo: do exit routine
                    break;
                }
            }
            Environment.Exit(0);
        }

        public static void LoadConfigurationFile(){
			Console.WriteLine("Loading Configuration");
			Config config = new Config("config.ini");
			foreach (Config.KeyValue keyvalue in config.GetConfigs())
			{
                switch (keyvalue.Key.ToLower())
				{
                    case "name":
                        Globals.Name = keyvalue.Value;
                        break;
					case "port":
                        int.TryParse(keyvalue.Value, out Globals.Port);
						break;
                    case "admins":
                        foreach(string id in keyvalue.Value.Split(',')){
                            if (long.TryParse(id, out long Id))
                            {
                                Globals.Admins.Add(Id);
                            }
                        }
                        break;
				}
			}
            Console.WriteLine("Loading Configuration done.");
        }

        public static void ApplyStartupArgs(string[] args){
			for (int i = 0; i < args.Length; i++)
			{
				if (args[i] == "-p")
				{
					int.TryParse(args[i + 1], out Globals.Port);
				}
			}
        }

        public static void OnServerReceive(object sender, NetworkReceiveEventArgs e)
        {
            if (!_accounts.ContainsKey(e.Sender))
            {
                _accounts.Add(e.Sender, new Account());
            }
            Account account = _accounts[e.Sender];

            switch (NetworkHelper.GetPackageType(e.Data))
            {
                #region "Login"
                case PackageType.Login:
                    LoginRequest request = NetworkHelper.Deserialize<LoginRequest>(e.Data);
                    if (account.Login(request.Username, request.Password))
                    {
                        _server.SendMessage(e.Sender, NetworkHelper.Serialize(new LoginResponse() { Success = true }));
                    }
                    else
                    {
                        _server.SendMessage(e.Sender, NetworkHelper.Serialize(new LoginResponse()));
                    }
                    break;
                #endregion
                #region "Logout"
                case PackageType.Logout:
                    LogoutRequest lr = NetworkHelper.Deserialize<LogoutRequest>(e.Data);
                    if (lr.Status == LogoutStatus.CharacterSelection)
                    {
                        account.LeaveWorld();
                        _server.SendMessage(e.Sender, NetworkHelper.Serialize(new LogoutResponse() { Success = true, Status = LogoutStatus.CharacterSelection }));
                    }
                    else if (lr.Status == LogoutStatus.TitleScreen)
                    {
                        account.Logout();
                        _server.SendMessage(e.Sender, NetworkHelper.Serialize(new LogoutResponse() { Success = true, Status = LogoutStatus.TitleScreen }));
                    }
                    break;
                #endregion
                #region "GetAccountCharacters"
                case PackageType.GetAccountCharacters:
                    GetAccountCharactersResponse gacr = new GetAccountCharactersResponse() { Success = true, Characters = account.GetCharacters() };
                    _server.SendMessage(e.Sender, NetworkHelper.Serialize(gacr));
                    break;
                #endregion
                #region "SelectCharacter"
                case PackageType.SelectCharacter:
                    SelectCharacterRequest selectCharacterRequest = NetworkHelper.Deserialize<SelectCharacterRequest>(e.Data);
                    SelectCharacterResponse scr = new SelectCharacterResponse();
                    if (account.JoinWorld(Character.GetCharacterById(selectCharacterRequest.CharacterId)))
                    {
                        scr.Success = true;
                    }
                    _server.SendMessage(e.Sender, NetworkHelper.Serialize(scr));
                    break;
                #endregion
                default:
                    Console.WriteLine("Received Unknown Package");
                    break;
            }
        }


    }
}
