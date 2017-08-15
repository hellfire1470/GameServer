using System;
using GameServer.IO;
using System.Collections.Generic;

namespace GameServer
{
    public static class BasicCommands
    {
        #region "Quit"
        public static Command Quit = new Command(new string[] { "quit", "stop", "exit", "close" }, (string[] args) =>
        {
            Environment.Stop();
        }, "Closes the server.");
        #endregion
        #region "ChangeName"
        public static Command ChangeName = new Command(new string[] { "name" }, (string[] args) =>
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
            Global.ConfigurationFile.SetValue("name", name);
            Global.ConfigurationFile.Apply();
            Global.Name = name;
            Console.WriteLine("Servername changed to: " + name);


        }, "Changes the name of this server. Type 'name ?' to get more information");
        #endregion
        #region "ChangePort"
        public static Command ChangePort = new Command(new string[] { "port" }, (string[] args) =>
        {
            int port = 0;
            if (int.TryParse(args[0], out port) && port >= 0 && port <= 65535)
            {
                Global.Port = port;
                Global.ConfigurationFile.SetValue("port", port.ToString());
                Global.ConfigurationFile.Apply();
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
        public static Command Help = new Command(new string[] { "?", "help" }, (string[] args) =>
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

            foreach (Command cmd in CommandManager.Commands)
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
        public static Command Online = new Command(new string[] { "online" }, (string[] args) =>
        {
            Console.WriteLine(MainClass.Server.ServerUser.Count + " Clients are connected to the server");
            int logged_in = 0;
            int ingame = 0;
            foreach (Server.Connection serverUser in MainClass.Server.ServerUser.Values)
            {
                Server.Account account = serverUser.Account;
                if (account == null)
                    continue;
                if (account.Authentificated)
                {
                    logged_in++;
                }
                if (account.InGame)
                {
                    ingame++;
                }
            }
            Console.WriteLine(logged_in + " Accounts are logged in");
            Console.WriteLine(ingame + " Accounts are ingame");
        }, "Show online players");
        #endregion
        #region "show"
        public static Command Show = new Command(new string[] { "show" }, (string[] args) =>
        {
            if (args.Length > 1)
            {
                if (args[0] == "account")
                {
                    Account.Action(new string[] { "show", args[1] });
                }
                if (args[0] == "character")
                {
                    Character.Action(new string[] { "show", args[1] });
                }
            }
        });
        #endregion
        #region "account"
        public static Command Account = new Command(new string[] { "account" }, (string[] args) =>
      {
          if (args.Length > 1)
          {
              if (args[0] == "create")
              {
                  if (args.Length == 3)
                  {
                      string username = args[1];
                      string password = args[2];
                      if (username.Trim(' ') != "" && password.Trim(' ') != "")
                      {
                          if (SQL.AccountData.Create(username, password))
                          {
                              Console.WriteLine("Account " + username + " successfully created");
                          }
                          else
                          {
                              Console.WriteLine("Failed to create account " + username);
                          }
                      }
                  }
                  else
                  {
                      Console.WriteLine("Wrong usage of command account create.");
                      Console.WriteLine("usage: account create <username> <password>");
                  }
              }
              else if (args[0] == "show")
              {
                  string accountId = args[1];
                  int id = 0;
                  SQL.AccountData account;
                  if (int.TryParse(accountId, out id))
                  {
                      account = new SQL.AccountData(id);
                  }
                  else
                  {
                      account = new SQL.AccountData(accountId);
                  }
                  if (account != null)
                  {
                      Console.WriteLine("Here is the dataset for " + accountId);
                      Console.WriteLine("id".PadRight(20) + account.Id.ToString());
                      Console.WriteLine("name".PadRight(20) + account.Username);
                      Console.WriteLine("password".PadRight(20) + account.Password);
                      Console.WriteLine(" ### Characters ### ");
                      foreach (SQL.Character character in SQL.AccountData.GetCharacters(account.Id))
                      {
                          Console.WriteLine(character.Id + " - " + character.Name);
                      }
                  }
                  else
                  {
                      Console.WriteLine("No dataset found for " + accountId);
                  }
              }
          }
      });
        #endregion
        #region "character"
        public static Command Character = new Command(new string[] { "character" }, (string[] args) =>
        {
            if (args.Length > 1)
            {
                if (args[0] == "show")
                {
                    int id = 0;
                    if (int.TryParse(args[1], out id))
                    {
                        SQL.Character character = SQL.Character.Load(id);
                        if (character != null)
                        {
                            Console.WriteLine("Here is the dataset for character " + id);
                            Console.WriteLine("Name".PadRight(20) + character.Name);
                            Console.WriteLine("Level".PadRight(20) + character.Level);
                            Console.WriteLine("Exp".PadRight(20) + character.Exp);
                            Console.WriteLine("Race".PadRight(20) + character.Race.ToString());
                            Console.WriteLine("Class".PadRight(20) + character.Class.ToString());
                            Console.WriteLine("Fraction".PadRight(20) + character.Fraction.ToString());
                            Console.WriteLine("Map".PadRight(20) + character.Location.Map.Name);
                            Console.WriteLine(" ### META ### ");
                            foreach (Dictionary<string, string> meta in character.Meta.Values)
                            {
                                Console.WriteLine(meta["key"].PadRight(20) + meta["value"]);
                            }
                            Console.WriteLine(" ### Account ### ");
                            Console.WriteLine("Id".PadRight(20) + character.AccountId);
                            Console.WriteLine("Username".PadRight(20) + new SQL.AccountData(character.AccountId).Username);
                        }
                        else
                        {
                            Console.WriteLine("No dataset found for character " + id);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Wrong usage: show character <id>");
                    }
                }
            }
        });
        #endregion
        #region "stats"
        public static Command Stats = new Command(new string[] { "stats" }, (string[] args) =>
        {

        });
        #endregion
    }
}
