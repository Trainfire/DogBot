using System;
using SteamKit2;

namespace Core
{
    public abstract class Module : IBot
    {
        public Bot Bot { get; private set; }
        public Logger Logger { get; private set; }

        public virtual void Initialize(Bot bot)
        {
            Bot = bot;
            Logger = new Logger(GetType().Name + ".log");
        }

        public virtual void OnDisconnect() { }
        public virtual void OnJoinChat(SteamID chatroomID) { }
        public virtual void OnLeaveChat(SteamID chatroomID) { }
        public virtual void OnLoggedIn() { }
        public virtual void OnLoggedOut() { }
        public virtual void OnNoActivity() { }
        public virtual void OnReceiveChatMessage(SteamFriends.ChatMsgCallback caller) { }
        public virtual void OnReceiveFriendMessage(SteamFriends.FriendMsgCallback caller) { }
    }
}
