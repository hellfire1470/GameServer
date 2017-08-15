using System.Collections.Generic;

namespace GameServer.IO
{
    public class Command
    {
        public delegate void CommandAction(string[] args);

        public List<string> Keywords { get; private set; }
        public CommandAction Action { get; private set; }
        public string Description { get; private set; }
        public Command(string[] keywords, CommandAction action, string description = "")
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

}
