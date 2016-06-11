using System;
using System.Collections.Generic;
using System.Linq;
using SteamKit2;

namespace DogBot
{
    public abstract class Command
    {
        public bool AdminOnly { get; set; }
        public string Alias { get; private set; }
        public List<string> HelpArgs { get; set; }
        public string Help
        {
            get
            {
                var formattedArgs = new List<string>();
                formattedArgs.Add("!" + Alias + "");
                HelpArgs.ForEach(x => formattedArgs.Add(string.Format("<{0}>", x)));
                return string.Join(" ", formattedArgs);
            }
        }

        public Command(string alias)
        {
            Alias = alias;
            HelpArgs = new List<string>();
        }

        public Command(string alias, params string[] args) : this(alias)
        {
            HelpArgs = args.ToList();
        }

        public abstract string Execute(DogBot bot, SteamID player, string message);
    }

    public class Command<T> : Command where T : CommandAction
    {
        public Command(string alias) : base(alias)
        {

        }

        /// <summary>
        /// Executes the command. A string value may be returned.
        /// </summary>
        public override string Execute(DogBot bot, SteamID player, string message)
        {
            var action = Activator.CreateInstance<T>();
            return action.Execute(bot, player, message);
        }
    }

    public abstract class CommandAction
    {
        /// <summary>
        /// Implement the action of a command here. Optionally, you can return a string to indicate a result.
        /// </summary>
        public abstract string Execute(DogBot bot, SteamID caller, string message);
    }
}
