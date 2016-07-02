using System.Collections.Generic;
using SteamKit2;

namespace Core
{
    public abstract class Module : ISteamKitCallbackHandler
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

        public virtual void OnDisconnect(SteamClient.DisconnectedCallback callback) { }
        public virtual void OnJoinChat(SteamFriends.ChatEnterCallback callback) { }
        public virtual void OnLeaveChat() { }
        public virtual void OnLoggedIn() { }
        public virtual void OnLoggedOut() { }
        public virtual void OnNoActivity() { }
        public virtual void OnReceiveChatMessage(SteamFriends.ChatMsgCallback caller) { }
        public virtual void OnReceiveFriendMessage(SteamFriends.FriendMsgCallback caller) { }
        public virtual void OnChatAction(SteamFriends.ChatActionResultCallback callback) { }
    }
}
