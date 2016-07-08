using Core;
using System.Timers;
using SteamKit2;
using System.Threading.Tasks;
using System;

namespace Modules.ChatJoiner
{
    public class ChatJoiner : Module, ILogOnCallbackHandler
    {
        Config config;
        Timer inactivityTimer;
        ulong chatRoomId;

        protected override void OnInitialize()
        {
            config = new Config();

            if (!IsConfigValid())
                return;

            inactivityTimer = new Timer();
            inactivityTimer.Interval = 1000 * config.Data.RejoinInterval;
            inactivityTimer.Elapsed += OnNoActivity;

            Bot.CallbackManager.Subscribe<SteamFriends.ChatEnterCallback>(OnJoinChat);
            Bot.CallbackManager.Subscribe<SteamFriends.ChatMsgCallback>(OnReceiveChatMessage);
            Bot.RegisterLogOnListener(this);
        }

        void OnJoinChat(SteamFriends.ChatEnterCallback callback)
        {
            if (callback.EnterResponse == EChatRoomEnterResponse.Success)
            {
                Logger.Info("Successfully joined chat '{0}' containing {1} members.", callback.ChatRoomName, callback.ChatMembers.Count);

                // Start the inactivity timer.
                inactivityTimer.Start();
            }
            else
            {
                Logger.Error("Failed to join the chat room. Reason: {0}", callback.EnterResponse);
            }
        }

        void OnReceiveChatMessage(SteamFriends.ChatMsgCallback caller)
        {
            inactivityTimer.Stop();
            inactivityTimer.Start();
        }

        void OnNoActivity(object sender, ElapsedEventArgs e)
        {
            Rejoin();
        }

        async void Rejoin()
        {
            inactivityTimer.Stop();

            Logger.Info("Rejoining chat due to inactivity");

            Logger.Info("Leaving...");
            Bot.Friends.LeaveChat(chatRoomId);

            var waitTime = TimeSpan.FromSeconds(config.Data.RejoinIntermission);
            Logger.Info("Waiting {0} seconds before rejoin...", waitTime.Seconds);

            await Task.Delay(waitTime);

            Logger.Info("Joining...");
            Bot.Friends.JoinChat(chatRoomId);
        }

        void ILogOnCallbackHandler.OnLoggedOn()
        {
            // Attempt to join chat.
            if (chatRoomId == 0)
            {
                Logger.Error("{0} is an invalid chat room ID", config.Data.ChatRoomId);
            }
            else
            {
                Logger.Info("Joining chat...");
                Bot.Friends.JoinChat(chatRoomId);
            }
        }

        bool IsConfigValid()
        {
            ulong.TryParse(config.Data.ChatRoomId, out chatRoomId);

            if (chatRoomId == 0)
            {
                Logger.Error("The chat room ID '{0}' is invalid.", chatRoomId);
                return false;
            }

            if (config.Data.RejoinInterval <= 0)
            {
                Logger.Error("Rejoin interval cannot be 0.");
                return false;
            }

            if (config.Data.RejoinIntermission <= 0)
            {
                Logger.Error("Rejoin intermission cannot be 0.");
                return false;
            }

            return true;
        }
    }
}
