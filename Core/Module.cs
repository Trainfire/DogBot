using System.Collections.Generic;
using SteamKit2;

namespace Core
{
    public abstract class Module
    {
        public Bot Bot { get; private set; }
        public Logger Logger { get; private set; }
        public virtual List<Command> Commands { get { return new List<Command>(); } }

        public void Initialize(Bot bot)
        {
            Bot = bot;
            Logger = new Logger(GetType().Name + ".log", GetType().Name);
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
