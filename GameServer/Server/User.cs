using System;
using System.Threading;
using System.Net;

namespace GameServer.Server
{
    public class User
    {
        public static double TimeoutTimeInMinutes = 1;

        public IPEndPoint IPEndPoint { get; private set; }
        public Account Account { get; private set; }
        public DateTime LastActivity { get; private set; }

        public event Action OnTimeout;

        private Thread TimeoutThread;


        public User(Account account, IPEndPoint ipendpoint)
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
}
