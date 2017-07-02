using System;
using System.Net;
using System.Threading;

namespace Network
{

    public class NetworkReceiveEventArgs : EventArgs
    {
        public byte[] Data { get; set; }
        public IPEndPoint Sender { get; set; }
    }

    public class UdpServer
    {
        private int _port;
        private IPEndPoint _listener;
        private System.Net.Sockets.UdpClient _server;

        private Thread _receiveThread;

        public event EventHandler<NetworkReceiveEventArgs> DataReceived;

        private bool _started = false;

        public UdpServer(int port)
        {
            _port = port;
        }

        public void Start()
        {
            if (!_started)
            {
                _started = true;
                _listener = new IPEndPoint(IPAddress.Any, _port);
                _server = new System.Net.Sockets.UdpClient(_listener);

                _receiveThread = new Thread(ReceiveData);
                _receiveThread.Start();
            } else {
                throw new Exception("Server already running");
            }
        }


        protected virtual void OnDataReceived(IPEndPoint sender, byte[] data)
        {
            if (DataReceived != null)
            {
                DataReceived(this, new NetworkReceiveEventArgs()
                {
                    Data = data,
                    Sender = sender
                });
            }
        }

        private void ReceiveData()
        {
            while (true)
            {
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = new byte[1024];
                data = _server.Receive(ref sender);
                OnDataReceived(sender, data);
            }
        }

        public void SendBytes(IPEndPoint address, byte[] data)
        {
            _server.Send(data, data.Length, address);
        }
    }
}
