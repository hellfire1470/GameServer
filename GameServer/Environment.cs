using System;
namespace GameServer
{
    public static class Environment
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
}
