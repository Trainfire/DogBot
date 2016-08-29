using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core
{
    /// <summary>
    /// Base class for a Command.
    /// </summary>
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

        public virtual string Execute(string message) { return string.Empty; }
        public virtual async Task<string> ExecuteAsync(string message) { return await Task.FromResult<string>(string.Empty); }
    }
}
