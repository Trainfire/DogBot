using System.Collections.Generic;
using SteamKit2;

namespace Core
{
    public abstract class Module : ICommandHandler
    {
        public Bot Bot { get; private set; }
        public Logger Logger { get; private set; }

        protected CommandParser CommandHandler { get; private set; }
        protected CommandListener CommandListener { get; private set; }

        public void Initialize(Bot bot)
        {
            Bot = bot;
            Logger = new Logger(bot.LogPath, GetType().Name);
            CommandHandler = new CommandParser(Bot);
            CommandListener = new CommandListener(Bot);
            OnInitialize();
        }

        public void Stop()
        {
            OnStop();
        }

        protected virtual void OnInitialize() { }
        protected virtual void OnStop() { }

        protected void AddCommand<TCommand>(string alias) where TCommand : ChatCommand
        {
            CommandListener.AddCommand<TCommand>(alias, this);
        }

        void ICommandHandler.OnCommandTriggered(CommandEvent commandEvent)
        {
            CommandHandler.ProcessCommand(commandEvent);
        }
    }
}
