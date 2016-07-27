using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core
{
    public abstract class Command
    {
        public virtual bool IsAsync { get { return false; } }
        public virtual bool UsersOnly { get { return false; } }
        public virtual bool AdminOnly { get { return false; } }
        public string Alias { get; set; }
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

        public Command()
        {

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

        public virtual CommandResult Execute(string message) { return new CommandResult(); }
        public virtual async Task<CommandResult> ExecuteAsync(string message) { return await Task.FromResult<CommandResult>(null); }
    }

    /// <summary>
    /// The result of an executed command, containing a message to be displayed.
    /// </summary>
    public class CommandResult
    {
        /// <summary>
        /// The message to be displayed in chat.
        /// </summary>
        public string Message { get; set; }

        public CommandResult()
        {

        }

        public CommandResult(string message)
        {
            Message = message;
        }

        public CommandResult(string message, params object[] args)
        {
            Message = string.Format(message, args);
        }
    }
}
