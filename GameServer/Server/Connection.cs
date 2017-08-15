using System;
using System.Threading;
using System.Net;

namespace GameServer.Server
{
    public class Connection
    {
        public static double TimeoutTimeInMinutes = 1;

        public IPEndPoint IPEndPoint { get; private set; }
        public Account Account { get; private set; }
        public DateTime LastActivity { get; private set; }

        public event Action OnTimeout;

        private Thread TimeoutThread;


        public Connection(IPEndPoint ipendpoint)
        {
            IPEndPoint = ipendpoint;
            RefreshActivity();

            TimeoutThread = new Thread(TimeoutThreadFunction);
            TimeoutThread.Start();
        }

        public void SetAccount(Account account)
        {
            Account = account;
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
}
