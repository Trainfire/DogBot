using SteamKit2;
using System.Collections.Generic;

namespace Core
{
    public class LoginStateHandler
    {
        readonly CallbackManager callbackManager;
        readonly List<ILogOnCallbackHandler> logOnListeners;
        readonly List<ILogOffCallbackHandler> logOffListeners;

        public LoginStateHandler(CallbackManager callbackManager)
        {
            this.callbackManager = callbackManager;

            logOnListeners = new List<ILogOnCallbackHandler>();
            logOffListeners = new List<ILogOffCallbackHandler>();

            // Subscribe to callbacks here.
            callbackManager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
        }

        public void RegisterLogOnListener(ILogOnCallbackHandler handler)
        {
            logOnListeners.Add(handler);
        }

        public void UnregisterLogOnListener(ILogOnCallbackHandler handler)
        {
            if (logOnListeners.Contains(handler))
                logOnListeners.Remove(handler);
        }

        public void RegisterLogOffListener(ILogOffCallbackHandler handler)
        {
            logOffListeners.Add(handler);
        }

        public void UnregisterLogOffListener(ILogOffCallbackHandler handler)
        {
            if (logOffListeners.Contains(handler))
                logOffListeners.Remove(handler);
        }

        void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {
            if (callback.Result == EResult.OK)
            {
                logOnListeners.ForEach(x => x.OnLoggedOn());
            }
            else
            {
                logOffListeners.ForEach(x => x.OnLoggedOff());
            }
        }
    }
}
