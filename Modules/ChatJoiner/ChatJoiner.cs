using Core;
using System.Timers;
using SteamKit2;

namespace Modules.ChatJoiner
{
    public class ChatJoiner : Module
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
        }

        public override void OnLoggedIn()
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
                    Bot.JoinChat(chatRoomId);
                }
            }
            else
            {
                Logger.Error("Could not connect to chat room as the chat room ID is invalid.");
            }
        }

        public override void OnJoinChat(SteamFriends.ChatEnterCallback callback)
        {
            // Start the inactivity timer.
            inactivityTimer.Start();
        }

        public override void OnChatAction(SteamFriends.ChatActionResultCallback callback)
        {
            Logger.Info(callback.ChatterID.ToString());
        }

        public override void OnReceiveChatMessage(SteamFriends.ChatMsgCallback caller)
        {
            inactivityTimer.Stop();
            inactivityTimer.Start();
        }

        void OnNoActivity(object sender, ElapsedEventArgs e)
        {
            Logger.Info("Rejoining chat due to inactivity");
            Bot.LeaveChat(chatRoomId);
            Bot.JoinChat(chatRoomId);
        }
    }
}
