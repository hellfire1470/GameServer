using System;
using System.Collections.Generic;

namespace GameServer.IO
{
    public static class CommandManager
    {
        private static readonly List<Command> _commands = new List<Command>();
        public static List<Command> Commands { get { return _commands; } }

        public static void AddCommand(Command cmd)
        {
            _commands.Add(cmd);
        }

        public static void HandleCommand(string input)
        {
            string[] args = input.Split(' ');
            string inputKeyword = args[0];

            string[] cmdArgs = new string[args.Length - 1];
            Array.Copy(args, 1, cmdArgs, 0, cmdArgs.Length);
            foreach (Command cmd in _commands)
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
