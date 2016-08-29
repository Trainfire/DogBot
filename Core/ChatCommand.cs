using Core;
using System.Threading.Tasks;

namespace Core
{
    /// <summary>
    /// Base class for a command executed via a Chat or Friend message.
    /// </summary>
    public abstract class ChatCommand : Command
    {
        public Bot Bot { get; private set; }
        public string Result { get; private set; }

        public virtual void Initialize(Bot bot)
        {
            Bot = bot;
        }

        public virtual string Execute(CommandSource source)
        {
            return string.Empty;
        }

        public virtual async Task<string> ExecuteAsync(CommandSource source)
        {
            return await Task.FromResult<string>(string.Empty);
        }
    }
}
