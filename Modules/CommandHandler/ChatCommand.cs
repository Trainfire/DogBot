using Core;

namespace Modules.CommandHandler
{
    /// <summary>
    /// A command executed via a Chat or Friend message.
    /// </summary>
    public abstract class ChatCommand : Command
    {
        public Bot Bot { get; private set; }
        public CommandResult Result { get; private set; }

        public virtual void Initialize(Bot bot)
        {
            Bot = bot;
        }

        public virtual CommandResult Execute(CommandSource source)
        {
            return new CommandResult();
        }
    }
}
