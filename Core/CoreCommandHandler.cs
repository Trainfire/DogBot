using Modules.CommandHandler;

namespace Core
{
    /// <summary>
    /// Adds and processes the core bot commands.
    /// </summary>
    public class CoreCommandHandler : ICommandListener
    {
        readonly ChatCommandProcessor commandProcessor;

        #region Commands
        const string PREFIX = "core";
        const string ADDUSER = "adduser";
        const string REMOVEUSER = "removeuser";
        const string EUSERVER = "euserver";
        #endregion

        public CoreCommandHandler(Bot bot)
        {
            commandProcessor = new ChatCommandProcessor(bot);

            var listener = bot.GetOrAddModule<CommandListener>();
            listener.AddCommand<AddUser>(FormatCommand(ADDUSER), this);
            listener.AddCommand<RemoveUser>(FormatCommand(REMOVEUSER), this);
            listener.AddCommand<ServerQuery>(FormatCommand(EUSERVER), this);
        }

        void ICommandListener.OnCommandTriggered(CommandEvent commandEvent)
        {
            commandProcessor.ProcessCommand(commandEvent);
        }

        public string FormatCommand(string alias)
        {
            return PREFIX + alias;
        }
    }
}
