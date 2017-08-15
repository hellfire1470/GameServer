using System;
using System.Collections.Generic;
using FileManager;
using GameServer.IO;

namespace GameServer
{


    static class Settings
    {
        public const string configuration_file = "config.ini";
        public const string database_file = "data.sqlite";
        public static string LogDirectory { get; private set; }
        public static string LogFile { get; private set; }
        public const double timeout_in_minutes = 15d;

        public static void SetLogDirectory(string directory)
        {
            LogDirectory = directory;
            Logger.LogDir = directory;
        }

        public static void SetLogFile(string file)
        {
            LogFile = file;
            Logger.LogFile = file;
        }

        public static void EnableLogging()
        {
            Logger.LogToFile = true;
        }

        public static void DisableLogging()
        {
            Logger.LogToFile = false;
        }
    }

    static class Global
    {
        public static Config ConfigurationFile;

        public static SQL.Base SqlBase;

        public static string Name = "Example Name";
        public static int Port = 5090;
        public static List<long> Admins = new List<long>();
    }

    class MainClass
    {
        private static Server.Server _server = new Server.Server();

        public static Server.Server Server { get { return _server; } }

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
                    CommandManager.HandleCommand(input);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.Message);
                }
            }
        }

        public static void StartupServer(string[] args)
        {
            DateTime startTime = DateTime.Now;

            GameServer.Server.Connection.TimeoutTimeInMinutes = Settings.timeout_in_minutes;

            Settings.SetLogDirectory("logs/");
            Settings.SetLogFile(DateTime.Now.ToString("u") + ".log");

            // Configuration File
            LoadConfigurationFile();

            // Database
            LoadDatabaseFile();


            ApplyStartupArgs(args);

            SetupCommands();

            _server.Start();

            Logger.Log("Server started");
            Logger.Log("Startup took " + DateTime.Now.Subtract(startTime).TotalSeconds + " seconds");
            Logger.Log("Server Name: " + Global.Name);
        }

        public static void LoadDatabaseFile()
        {
            Global.SqlBase = new SQL.Base();
            Logger.Log("Loading Maps ... ", true);
            MapManager.LoadMaps(Global.SqlBase);
            Logger.Log("Done");
        }

        public static void SetupCommands()
        {
            Logger.Log("Setup commands ... ", true);
            CommandManager.AddCommand(BasicCommands.Quit);
            CommandManager.AddCommand(BasicCommands.ChangeName);
            CommandManager.AddCommand(BasicCommands.Online);
            CommandManager.AddCommand(BasicCommands.ChangePort);
            CommandManager.AddCommand(BasicCommands.Help);
            CommandManager.AddCommand(BasicCommands.Show);
            CommandManager.AddCommand(BasicCommands.Account);
            CommandManager.AddCommand(BasicCommands.Character);
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
                        Settings.SetLogDirectory(keyvalue.Value);
                        break;
                    case "log_file":
                        Settings.SetLogFile(keyvalue.Value);
                        break;
                    case "log_to_file":
                        if (bool.Parse(keyvalue.Value))
                            Settings.EnableLogging();
                        else
                            Settings.DisableLogging();
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
        }



    }
}
