using System.Collections.Generic;
using System.Linq;

namespace Core
{
    public class CommandRegistry
    {
        public List<Command> Commands { get; private set; }
        public string Token { get; private set; }
        public string Prefix { get; private set; }

        public CommandRegistry(string token, string prefix)
        {
            Commands = new List<Command>();
            Token = token;
        }

        public void AddCommand(Command command)
        {
            Commands.Add(command);
        }

        public Command GetCommand(string command)
        {
            return Commands.FirstOrDefault(x => x.Alias == command);
        }

        /// <summary>
        /// Formats the specified string into a command. If the command is 'help' and the command token is '!', the return value would be '!help'.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public string Format(string command)
        {
            return string.Format("{0}{1}", Token, command);
        }
    }
}
