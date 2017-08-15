using System;
using System.Collections.Generic;

namespace GameServer.IO
{
    public static class CommandManager
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
            Logger.Log("Command '" + inputKeyword + "' not found.");
        }
    }
}
