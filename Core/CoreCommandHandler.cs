namespace Core
{
    /// <summary>
    /// Adds and processes the core bot commands.
    /// </summary>
    public class CoreCommandHandler : ICommandHandler
    {
        readonly CommandParser commandProcessor;

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
            commandProcessor = new CommandParser(bot);

            var listener = new CommandListener(bot);
            listener.AddCommand<AddUser>(FormatCommand(ADDUSER), this);
            listener.AddCommand<RemoveUser>(FormatCommand(REMOVEUSER), this);
            listener.AddCommand<SetName>(FormatCommand(SETNAME), this);
            listener.AddCommand<ServerQuery>(FormatCommand(EUSERVER), this, (command) =>
            {
                command.ServerName = "EU Server";
                command.Hostname = "geit.uk";
                command.Port = 27015;
            });
            listener.AddCommand<ServerQuery>(FormatCommand(USSERVER), this, (command) =>
            {
                command.ServerName = "US Server";
                command.IPAddress = "70.42.74.31";
                command.Port = 27015;
            });
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
