using System.Collections.Generic;
using SteamKit2;

namespace Core
{
    public abstract class Module
    {
        public Bot Bot { get; private set; }
        public Logger Logger { get; private set; }

        protected CommandListener CommandListener
        {
            get { return Bot.Listener; }
        }

        public void Initialize(Bot bot)
        {
            Bot = bot;
            Logger = new Logger(bot.LogPath, GetType().Name);
            OnInitialize();
        }

        public void Stop()
        {
            OnStop();
        }

        protected virtual void OnInitialize() { }
        protected virtual void OnStop() { }
    }
}
