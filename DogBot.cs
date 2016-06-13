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
        readonly Timer inactivityTimer;
        readonly Logger logger;

        const string LOGPATH = "log.bin";

        SteamID chatId;
        bool muted;

        public BotData Data { get; private set; }
        public SteamID SID { get { return connection.User.SteamID; } }

        public DogBot()
        {
            config = Config.Load();

            logger = new Logger(LOGPATH, config.SteamName);
            logger.Info("Started");

            Data = new BotData();

            announcer = new Timer(1000 * config.AnnouncementInterval);
            announcer.Elapsed += OnAnnounce;

            inactivityTimer = new Timer(1000 * config.RejoinInterval);
            inactivityTimer.Elapsed += OnNoActivity;

            connection = new Connection();
            connection.LoggedOn += OnLoggedOn;
            connection.RecieveMessage += OnReceiveMessage;
            connection.Connect(config.User, config.Pass, config.SteamName);
        }

        void OnNoActivity(object sender, ElapsedEventArgs e)
        {
            logger.Info("Rejoining chat due to inactivity");
            connection.Friends.LeaveChat(chatId);
            connection.Friends.JoinChat(chatId);
        }

        void OnAnnounce(object sender, ElapsedEventArgs e)
        {
            // Post DoTD
            if (Data.Dog.IsSet)
            {
                logger.Info("Posting announcement...");
                HandleMessage(SID, CommandRegistry.Dotd);
            }
        }

        void OnLoggedOn(object sender, EventArgs e)
        {
            logger.Info("Logged on");

            // Attempt to join chat.
            if (!string.IsNullOrEmpty(config.ChatRoomId))
            {
                ulong chatRoomId = 0;
                ulong.TryParse(config.ChatRoomId, out chatRoomId);

                if (chatRoomId == 0)
                {
                    logger.Error("{0} is an invalid chat room ID", config.ChatRoomId);
                }
                else
                {
                    connection.Friends.JoinChat(chatRoomId);
                    chatId = new SteamID(chatRoomId);

                    // Start the announcer timer upon joining chat.
                    announcer.Start();

                    // Start the inactivity timer
                    inactivityTimer.Start();
                }
            }
            else
            {
                logger.Error("Could not connect to chat room as the chat room ID is invalid.");
            }
        }

        /// <summary>
        /// Called when a message is receieved in chat.
        /// </summary>
        void OnReceiveMessage(object sender, SteamFriends.ChatMsgCallback callback)
        {
            inactivityTimer.Stop();
            inactivityTimer.Start();
            HandleMessage(callback.ChatterID, callback.Message);
        }

        void HandleMessage(SteamID caller, string message)
        {
            // Process the received message and pass in the current Bot's data.
            var handler = new MessageHandler(this, caller, message);

            // Log info about the execution of the command.
            if (handler.Record.Executer.IsValid)
            {
                var steamName = connection.Friends.GetFriendPersonaName(handler.Record.Executer);
                logger.Info("Command Execution: '{0}' by {1}. Arguments: {2}", handler.Record.Command, steamName, handler.Record.Args);
            }

            // Echo the result if there is one.
            if (!string.IsNullOrEmpty(handler.Record.Result.FeedbackMessage))
                Say(chatId, handler.Record.Result.FeedbackMessage);

            // Log a log message if there is one.
            if (!string.IsNullOrEmpty(handler.Record.Result.LogMessage))
                logger.Info(handler.Record.Result.LogMessage);
        }

        void Say(SteamID chatId, string message)
        {
            if (!muted)
                connection.Friends.SendChatRoomMessage(chatId, EChatEntryType.ChatMsg, message);
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

        public void Mute()
        {
            Say(chatId, Strings.Muted);
            muted = true;
        }

        public void Unmute()
        {
            muted = false;
            Say(chatId, Strings.Unmuted);
        }
        #endregion
    }
}
