using Core;
using SteamKit2;

namespace Modules.Messenger
{
    public class Messenger : Module
    {
        public enum MessageContext
        {
            Chat,
            Friend,
        }

        public bool Muted { get; set; }

        protected override void OnInitialize()
        {
            Bot.CallbackManager.Subscribe<SteamFriends.ChatMsgCallback>(OnReceiveChatMessage);
            Bot.CallbackManager.Subscribe<SteamFriends.FriendMsgCallback>(OnReceiveFriendMessage);
        }

        void OnReceiveChatMessage(SteamFriends.ChatMsgCallback callback)
        {
            HandleMessage(MessageContext.Chat, callback.ChatterID, callback.Message);
        }

        void OnReceiveFriendMessage(SteamFriends.FriendMsgCallback callback)
        {
            HandleMessage(MessageContext.Friend, callback.Sender, callback.Message);
        }

        void HandleMessage(MessageContext context, SteamID caller, string message)
        {
            // Process the received message and pass in the current Bot's data.
            var handler = new MessageHandler(Bot, caller, message);

            if (handler.Record != null)
            {
                // Log info about the execution of the command.
                if (handler.Record.Executer.IsValid)
                {
                    var steamName = Bot.GetFriendName(handler.Record.Executer);
                    Logger.Info("Command Execution: From {0}: '{1}' by {2}. Arguments: {3}", context.ToString(), handler.Record.Command, steamName, handler.Record.Args);
                }

                // Echo the result if there is one.
                if (!string.IsNullOrEmpty(handler.Record.Result.FeedbackMessage))
                {
                    if (context == MessageContext.Chat)
                    {
                        SayToChat(Bot.CurrentChatRoomID, handler.Record.Result.FeedbackMessage);
                    }
                    else
                    {
                        SayToFriend(caller, handler.Record.Result.FeedbackMessage);
                    }
                }

                // Log a log message if there is one.
                if (!string.IsNullOrEmpty(handler.Record.Result.LogMessage))
                    Logger.Info(handler.Record.Result.LogMessage);
            }
        }

        void SayToChat(SteamID chatId, string message)
        {
            if (!Muted)
            {
                Bot.Friends.SendChatRoomMessage(chatId, EChatEntryType.ChatMsg, message);
                Logger.Info("@Chat: {0}", message);
            }
        }

        void SayToFriend(SteamID friend, string message)
        {
            Bot.Friends.SendChatMessage(friend, EChatEntryType.ChatMsg, message);
            Logger.Info("@{0}: {1}", Bot.GetFriendName(friend), message);
        }
    }
}
