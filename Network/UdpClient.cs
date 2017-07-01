using System;
using System.Net;
using System.Threading;

namespace Network
{
    public class UdpClient
    {
        private string _host;
        private int _port;
        private System.Net.Sockets.UdpClient _client;
        private IPEndPoint _endPoint;

        public event EventHandler<NetworkReceiveEventArgs> DataReceived;
        public event EventHandler Connected;
        public event EventHandler ConnectionLost;


        public bool IsConnected { get; private set; }

        public UdpClient(string host, int port)
        {
            _host = host;
            _port = 5090;
        }

        public void Connect()
        {
            _client = new System.Net.Sockets.UdpClient();
            _endPoint = new IPEndPoint(IPAddress.Parse(_host), _port);

            _client.Connect(_endPoint);

            Thread receiveThread = new Thread(ReceiveData);
            receiveThread.Start();

        }

        protected virtual void OnDataReceived(byte[] data)
        {
            if (DataReceived != null)
            {

                DataReceived(this, new NetworkReceiveEventArgs()
                {
                    Data = data,
                    Sender = _endPoint
                });
            }
        }

        public void ReceiveData()
        {
            while (true)
            {
                byte[] buffer = _client.Receive(ref _endPoint);
                OnDataReceived(buffer);
            }
        }

        public void SendMessage(byte[] data)
        {
            _client.Send(data, data.Length);
        }
    }
}
