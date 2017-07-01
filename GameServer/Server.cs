using System;
using Network;
using Network.Helper;
using Network.Packages;
using System.Collections.Generic;
using System.Net;

namespace GameServer
{
    class MainClass
    {

        public static Dictionary<IPEndPoint, Account> _accounts = new Dictionary<IPEndPoint, Account>();
        public static UdpServer _server;

        public static void Main(string[] args)
        {
            Console.WriteLine("Starting Server ...");
			int port = 5090;
			for (int i = 0; i < args.Length; i++)
			{
				if (args[i] == "-p")
				{
					port = int.Parse(args[i + 1]);
				}
			}
			Console.WriteLine("Listening on Port: " + port);
			_server = new UdpServer(port);
			_server.DataReceived += OnServerReceive;
			_server.Start();
			Console.WriteLine("Server started");

            while(true){
                string input = Console.ReadLine().ToLower();
                if(input == "quit"){
                    //todo: do exit routine
                    break;
                }
            }
            Environment.Exit(0);
        }

        public static void OnServerReceive(object sender, NetworkReceiveEventArgs e)
        {
            if(!_accounts.ContainsKey(e.Sender)){
                _accounts.Add(e.Sender, new Account());
            }
            Account account = _accounts[e.Sender];

            switch (NetworkHelper.GetPackageType(e.Data)){
#region "Login"
                case PackageType.Login:
                    LoginRequest request = NetworkHelper.Deserialize<LoginRequest>(e.Data);
                    if(account.Login(request.Username, request.Password)){
                        _server.SendMessage(e.Sender, NetworkHelper.Serialize(new LoginResponse(){Success = true}));
                    } else {
                        _server.SendMessage(e.Sender, NetworkHelper.Serialize(new LoginResponse()));
                    }
                    break;
                #endregion
                #region "Logout"
                case PackageType.Logout:
                    LogoutRequest lr = NetworkHelper.Deserialize<LogoutRequest>(e.Data);
                    if(lr.Status == LogoutStatus.CharacterSelection){
                        account.LeaveWorld();
                        _server.SendMessage(e.Sender, NetworkHelper.Serialize(new LogoutResponse(){Success = true, Status = LogoutStatus.CharacterSelection}));
                    } else if(lr.Status == LogoutStatus.TitleScreen){
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
                    if(account.JoinWorld(Character.GetCharacterById(selectCharacterRequest.CharacterId))){
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
