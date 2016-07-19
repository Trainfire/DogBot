namespace Core
{
    /// <summary>
    /// Adds and processes the core bot commands.
    /// </summary>
    public class CoreCommandHandler : ICommandHandler
    {
        readonly ChatCommandProcessor commandProcessor;

        #region Commands
        const string PREFIX = "~";
        const string ADDUSER = "adduser";
        const string REMOVEUSER = "removeuser";
        const string EUSERVER = "euserver";
        const string USSERVER = "usserver";
        const string SETNAME = "setname";
        #endregion

        public CoreCommandHandler(Bot bot)
        {
            commandProcessor = new ChatCommandProcessor(bot);

            var listener = new CommandListener(bot);
            listener.AddCommand<AddUser>(FormatCommand(ADDUSER), this);
            listener.AddCommand<RemoveUser>(FormatCommand(REMOVEUSER), this);
            listener.AddCommand<SetName>(FormatCommand(SETNAME), this);
            listener.AddCommand<EUServerQuery>(FormatCommand(EUSERVER), this);
            listener.AddCommand<USServerQuery>(FormatCommand(USSERVER), this);
        }

        void ICommandHandler.OnCommandTriggered(CommandEvent commandEvent)
        {
            commandProcessor.ProcessCommand(commandEvent);
        }

        public string FormatCommand(string alias)
        {
            return PREFIX + alias;
        }
    }
}
