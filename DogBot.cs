using System;
using System.Timers;
using SteamKit2;

namespace DogBot
{
    public class DogBot
    {
        readonly Connection connection;
        readonly ConfigData config;
        readonly Timer announcer;

        SteamID chatId;

        public BotData Data { get; private set; }
        public SteamID SID { get { return connection.User.SteamID; } }

        public DogBot()
        {
            config = Config.Load();
            Data = new BotData();

            announcer = new Timer(1000 * config.AnnouncementInterval);
            announcer.Elapsed += OnAnnounce;

            connection = new Connection();
            connection.LoggedOn += OnLoggedOn;
            connection.RecieveMessage += OnReceiveMessage;
            connection.Connect(config.User, config.Pass, config.SteamName);
        }

        void OnAnnounce(object sender, ElapsedEventArgs e)
        {
            // Post DoTD
            if (Data.Dog.IsSet)
                HandleMessage(SID, CommandRegistry.Dotd);
        }

        void OnLoggedOn(object sender, EventArgs e)
        {
            Log("Logged on");

            // Attempt to join chat.
            if (!string.IsNullOrEmpty(config.ChatRoomId))
            {
                ulong chatRoomId = 0;
                ulong.TryParse(config.ChatRoomId, out chatRoomId);

                if (chatRoomId == 0)
                {
                    Log("{0} is an invalid chat room ID", config.ChatRoomId);
                }
                else
                {
                    connection.Friends.JoinChat(chatRoomId);
                    chatId = new SteamID(chatRoomId);

                    // Start the announcer timer upon joining chat.
                    announcer.Start();
                }
            }
            else
            {
                Log("Could not connect to chat room as the chat room ID is invalid.");
            }
        }

        /// <summary>
        /// Called when a message is receieved in chat.
        /// </summary>
        void OnReceiveMessage(object sender, SteamFriends.ChatMsgCallback callback)
        {
            HandleMessage(callback.ChatterID, callback.Message);
        }

        void HandleMessage(SteamID caller, string message)
        {
            // Process the received message and pass in the current Bot's data.
            var handler = new MessageHandler(this, caller, message);

            // Echo the result if there is one.
            if (handler.Result != null)
                Say(chatId, handler.Result);
        }

        void Say(SteamID chatId, string message)
        {
            connection.Friends.SendChatRoomMessage(chatId, EChatEntryType.ChatMsg, message);
        }

        void Log(string message, params object[] args)
        {
            Console.WriteLine("[DogBot] " + string.Format(message, args));
        }

        #region Helpers
        public string GetFriendName(SteamID id)
        {
            return connection.Friends.GetFriendPersonaName(id);
        }

        public bool IsAdmin(SteamID id)
        {
            return config.Admins != null ? config.Admins.Contains(id.ToString()) : false;
        }
        #endregion
    }
}
