using System.Collections.Generic;

namespace Core
{
    public class CommandRegistry
    {
        public List<Command> Commands { get; private set; }
        public string Token { get; private set; }
        public string Prefix { get; private set; }

        #region Commands
        const string HELP = "help";
        const string STATS = "stats";
        const string MUTE = "mute";
        const string UNMUTE = "unmute";
        const string PERMISSION = "permission";
        const string POPULATENAMECACHE = "populatenamecache";
        #endregion

        public CommandRegistry(string token, string prefix)
        {
            Token = token;

            Commands = new List<Command>();

            // Default commands
            AddCommand(new Command<Help>(HELP));
            AddCommand(new Command<Mute>(MUTE)
            {
                AdminOnly = true,
            });
            AddCommand(new Command<Unmute>(UNMUTE)
            {
                AdminOnly = true,
            });
        }

        public void AddCommand(Command command)
        {
            Commands.Add(command);
        }

        public string Format(string command)
        {
            return string.Format("{0}{1}", Token, command);
        }
    }
}
