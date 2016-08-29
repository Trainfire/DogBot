using Core;
using System.Threading.Tasks;
using System;

namespace Core
{
    /// <summary>
    /// Base class for an anonymous command executed via a Chat or Friend message.
    /// </summary>
    public class AnonCommand : Command
    {
        public Bot Bot { get; private set; }
        public string Result { get; private set; }
        public Func<CommandSource, string> Func { get; private set; }

        public AnonCommand(string alias, Func<CommandSource, string> func)
        {
            Alias = alias;
            Func = func;
        }

        public virtual string Execute(CommandSource source)
        {
            return Func(source);
        }

        public virtual async Task<string> ExecuteAsync(CommandSource source)
        {
            return await Task.FromResult<string>(string.Empty);
        }
    }
}
