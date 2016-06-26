using System;
using System.Collections.Generic;
using System.Linq;
using SteamKit2;

namespace DogBot
{
    public abstract class Command
    {
        public bool UsersOnly { get; set; }
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

        public abstract CommandRecord Execute(DogBot bot, SteamID player, MessageParser parser);
    }

    public class Command<T> : Command where T : CommandAction
    {
        public Command(string alias) : base(alias)
        {

        }

        /// <summary>
        /// Executes the command. A string value may be returned.
        /// </summary>
        public override CommandRecord Execute(DogBot bot, SteamID player, MessageParser parser)
        {
            var action = Activator.CreateInstance<T>();
            return new CommandRecord(this, player, action.Execute(bot, player, parser), parser);
        }
    }

    public abstract class CommandAction
    {
        /// <summary>
        /// Implement the action of a command here. Optionally, you can return a string to indicate a result.
        /// </summary>
        public abstract CommandResult Execute(DogBot bot, SteamID caller, MessageParser parser);
    }

    /// <summary>
    /// A record of who executed the command, the specified arguments, and the result of the command.
    /// </summary>
    public class CommandRecord
    {
        readonly Command command;

        /// <summary>
        /// The name of the command that was executed.
        /// </summary>
        public string Command
        {
            get
            {
                return command != null ? command.Alias : "Unknown";
            }
        }

        public string Args
        {
            get
            {
                return string.Join(" ", Parser.Args);
            }
        }

        /// <summary>
        /// The Steam account that executed this command.
        /// </summary>
        public SteamID Executer { get; private set; }

        /// <summary>
        /// The result of executing this command.
        /// </summary>
        public CommandResult Result { get; private set; }

        /// <summary>
        /// The parser used to process this command.
        /// </summary>
        public MessageParser Parser { get; private set; }

        public CommandRecord(SteamID executer, CommandResult result, MessageParser parser)
        {
            Executer = executer;
            Result = result;
            Parser = parser;
        }

        public CommandRecord(Command command, SteamID execture, CommandResult result, MessageParser parser) : this(execture, result, parser)
        {
            this.command = command;
        }
    }

    /// <summary>
    /// The result of an executed command, containing a message to be displayed in chat and a message to be logged.
    /// </summary>
    public class CommandResult
    {
        /// <summary>
        /// The message to be displayed in chat.
        /// </summary>
        public string FeedbackMessage { get; set; }

        /// <summary>
        /// The message to be logged.
        /// </summary>
        public string LogMessage { get; set; }

        public string PrivateMessage { get; set; }

        public CommandResult()
        {

        }

        public CommandResult(string feedback, string log = "")
        {
            FeedbackMessage = feedback;
            LogMessage = log;
        }
    }
}
