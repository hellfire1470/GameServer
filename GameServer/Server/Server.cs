using System.Net;
using Network;
using System.Collections.Generic;
using GameServer.Network;

namespace GameServer.Server
{
    public class Server
    {
        private readonly Dictionary<IPEndPoint, Connection> _dictServerUser = new Dictionary<IPEndPoint, Connection>();
        private UdpServer _server;


        public Dictionary<IPEndPoint, Connection> ServerUser { get { return _dictServerUser; } }

        public void Start()
        {
            _server = new UdpServer(Global.Port);
            _server.DataReceived += OnServerReceive;
            _server.Start();

            PackageFunctions.SetServer(this);

            Logger.Log("Listening on Port: " + Global.Port);
        }

        public void SendBytes(IPEndPoint ipendpoint, byte[] bytes)
        {
            _server.SendBytes(ipendpoint, bytes);
        }

        private void OnServerReceive(object sender, NetworkReceiveEventArgs e)
        {
            if (!_dictServerUser.ContainsKey(e.Sender))
            {
                Connection serverUser = new Connection(e.Sender);
                serverUser.OnTimeout += () =>
                {
                    Logger.Log("[" + e.Sender.Address + "] was disconnected. Reason: Timeout");
                    serverUser.Account.Logout();
                    _dictServerUser.Remove(serverUser.IPEndPoint);
                };
                _dictServerUser.Add(e.Sender, serverUser);
            }

            _dictServerUser[e.Sender].RefreshActivity();
            PackageManager.HandlePackage(_dictServerUser[e.Sender], e);
        }
    }
}
