using System;
using System.Collections.Generic;

namespace GameServer
{

    public static class CommandManager
    {
        #region "Quit"
        public static Cmd Quit = new Cmd(new string[] { "quit" }, (string[] args) =>
        {
            Environment.Exit(0);
        }, "Closes the server.");
        #endregion
        #region "ChangeName"
        public static Cmd ChangeName = new Cmd(new string[] { "name" }, (string[] args) =>
        {
            string name = String.Join(" ", args);
            if (name == "?")
            {
                Console.WriteLine("Additional Help.");
                return;
            }
            if (name == "")
            {
                Console.WriteLine("Name can not be empty");
                return;
            }
            Globals.ConfigurationFile.SetValue("name", name);
            Globals.ConfigurationFile.Apply();
            Globals.Name = name;
            Console.WriteLine("Servername changed to: " + name);


        }, "Changes the name of this server. Type 'name ?' to get more information");
        #endregion
        #region "ChangePort"
        public static Cmd ChangePort = new Cmd(new string[] { "port" }, (string[] args) =>
        {
            int port = 0;
            if (int.TryParse(args[0], out port) && port >= 0 && port <= 65535)
            {
                Globals.Port = port;
                Globals.ConfigurationFile.SetValue("port", port.ToString());
                Globals.ConfigurationFile.Apply();
                Console.WriteLine("Port changed to " + port.ToString());
                Console.WriteLine("Please restart the server to apply changes");
            }
            else
            {
                Console.WriteLine("usage: port <port>");
                Console.WriteLine("<port> is a number between 0 and 65535");
            }
        }, "Changes the port to the given.\nUsage: port <port>\nExample: port 10");
        #endregion
        #region "Help"
        public static Cmd Help = new Cmd(new string[] { "?", "help" }, (string[] args) =>
        {


            string specificCmd = "";

            if (args.Length == 1)
            {
                specificCmd = args[0];
            }
            else
            {
                Console.WriteLine("\n\nFollowing commands are registered");
                Console.WriteLine("Type 'help <command>' to show information about the command");
            }

            foreach (Cmd cmd in CommandListener.Commands)
            {
                if (specificCmd != "" && !cmd.Keywords.Contains(specificCmd)) { continue; }

                Console.WriteLine(String.Join(", ", cmd.Keywords));
                if (specificCmd != "" && cmd.Description != "")
                {
                    Console.WriteLine(cmd.Description);
                }
            }
        }, "show list of commands");
        #endregion
        #region "Online"
        public static Cmd Online = new Cmd(new string[] { "online" }, (string[] args) =>
        {
            Console.WriteLine(MainClass._accounts.Count + " Clients are connected to the server");
            Console.WriteLine(MainClass._accounts.Count + " Accounts are logged in");
            Console.WriteLine(MainClass._accounts.Count + " Accounts are ingame");
        }, "Show online players");
        #endregion

        #region "show"
        public static Cmd Show = new Cmd(new string[] { "show" }, (string[] args) =>
        {
            if (args.Length > 1)
            {
                if (args[0] == "account")
                {
                    string account = args[1];
                    int id = 0;
                    SqlAccountData accountData;
                    if (int.TryParse(account, out id))
                    {
                        accountData = Globals.SqlBase.GetAccount(id);
                    }
                    else
                    {
                        accountData = Globals.SqlBase.GetAccount(account);
                    }
                    if (accountData != null)
                    {
                        Console.WriteLine("Here is the dataset for " + account);
                        Console.WriteLine("id".PadRight(20) + accountData.Id.ToString());
                        Console.WriteLine("name".PadRight(20) + accountData.Username);
                        Console.WriteLine("password".PadRight(20) + accountData.Password);
                    }
                    else
                    {
                        Console.WriteLine("No dataset found for " + account);
                    }
                }
                if (args[0] == "characters")
                {

                }
            }
        });
        #endregion

        #region "account"
        public static Cmd Account = new Cmd(new string[] { "account" }, (string[] args) => {
            if(args.Length > 1)
            {
                if(args[0] == "create"){
                    if(args.Length == 3){
                        string username = args[1];
                        string password = args[2];
                        if(username.Trim(' ') != "" && password.Trim(' ') != ""){
                            if(Globals.SqlBase.CreateAccount(username, password)){
                                Console.WriteLine("Account " + username + " successfully created");
                            } else {
                                Console.WriteLine("Failed to create account " + username);
                            }
                        }
                    } else {
                        Console.WriteLine("Wrong usage of command account create.");
                        Console.WriteLine("usage: account create <username> <password>");
                    }
                }
            }
        });
#endregion
    }

    public delegate void CmdAction(string[] args);

    public class Cmd
    {
        public List<string> Keywords { get; private set; }
        public CmdAction Action { get; private set; }
        public string Description { get; private set; }
        public Cmd(string[] keywords, CmdAction action, string description = "")
        {
            Keywords = new List<string>();
            foreach (string keyword in keywords)
            {
                Keywords.Add(keyword);
            }
            Action = action;
            Description = description;
        }
    }

    public static class CommandListener
    {
        private static readonly List<Cmd> _commands = new List<Cmd>();
        public static List<Cmd> Commands { get { return _commands; } }

        public static void AddCommand(Cmd cmd)
        {
            _commands.Add(cmd);
        }

        public static void HandleCommand(string input)
        {
            string[] args = input.Split(' ');
            string inputKeyword = args[0];

            string[] cmdArgs = new string[args.Length - 1];
            Array.Copy(args, 1, cmdArgs, 0, cmdArgs.Length);
            foreach (Cmd cmd in _commands)
            {
                if (cmd.Keywords.Contains(inputKeyword))
                {
                    cmd.Action(cmdArgs);
                    return;
                }
            }
            Console.WriteLine("Command '" + inputKeyword + "' not found.");
        }
    }
}
