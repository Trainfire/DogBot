using Core;
using System.Timers;
using SteamKit2;
using System;

namespace Modules.ChatJoiner
{
    public class ChatJoiner : Module, ILogOnCallbackHandler
    {
        Timer inactivityTimer;
        Config config;
        SteamID chatRoomId;

        protected override void OnInitialize()
        {
            config = new Config();

            inactivityTimer = new Timer();

            if (config.Data.RejoinInterval == 0)
            {
                Logger.Error("Rejoin interval cannot be 0.");
            }
            else
            {
                inactivityTimer.Interval = 1000 * config.Data.RejoinInterval;
                inactivityTimer.Elapsed += OnNoActivity;
            }

            chatRoomId = new SteamID(config.Data.ChatRoomId);

            Bot.CallbackManager.Subscribe<SteamFriends.ChatEnterCallback>(OnJoinChat);
            Bot.CallbackManager.Subscribe<SteamFriends.ChatMsgCallback>(OnReceiveChatMessage);
            Bot.RegisterLogOnListener(this);
        }

        void OnJoinChat(SteamFriends.ChatEnterCallback callback)
        {
            // Start the inactivity timer.
            inactivityTimer.Start();
        }

        void OnReceiveChatMessage(SteamFriends.ChatMsgCallback caller)
        {
            inactivityTimer.Stop();
            inactivityTimer.Start();
        }

        void OnNoActivity(object sender, ElapsedEventArgs e)
        {
            Logger.Info("Rejoining chat due to inactivity");
            Bot.Friends.LeaveChat(chatRoomId);
            Bot.Friends.JoinChat(chatRoomId);
        }

        void ILogOnCallbackHandler.OnLoggedOn()
        {
            // Attempt to join chat.
            if (!string.IsNullOrEmpty(config.Data.ChatRoomId))
            {
                ulong chatRoomId = 0;
                ulong.TryParse(config.Data.ChatRoomId, out chatRoomId);

                if (chatRoomId == 0)
                {
                    Logger.Error("{0} is an invalid chat room ID", config.Data.ChatRoomId);
                }
                else
                {
                    Logger.Info("Joining chat {0}", Bot.GetChatRoomName(chatRoomId));
                    Bot.Friends.JoinChat(chatRoomId);
                }
            }
            else
            {
                Logger.Error("Could not connect to chat room as the chat room ID is invalid.");
            }
        }
    }
}
