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

        private Thread _receiveThread;

        public event EventHandler<NetworkReceiveEventArgs> DataReceived;

        public UdpClient(string host, int port)
        {
            _host = host;
            _port = port;

        }

        public void Connect()
        {
            _client = new System.Net.Sockets.UdpClient();
            _endPoint = new IPEndPoint(IPAddress.Parse(_host), _port);

            _client.Connect(_endPoint);


            _receiveThread = new Thread(ReceiveData);
            _receiveThread.Start();
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

        private void ReceiveData()
        {
            while (true)
            {
                byte[] buffer = _client.Receive(ref _endPoint);
                OnDataReceived(buffer);
            }
        }

        public void SendBytes(byte[] data)
        {
            _client.Send(data, data.Length);
        }
    }
}
