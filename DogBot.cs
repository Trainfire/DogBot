using System;
using System.Timers;
using SteamKit2;

namespace DogBot
{
    public class DogBot
    {
        readonly Connection connection;
        readonly ConfigData config;
        readonly Timer inactivityTimer;
        readonly Announcer announcer;
        readonly Logger logger;
        readonly NameCache nameCache;

        public const string LOGPATH = "log.bin";

        SteamID chatId;
        bool muted;

        public BotData Data { get; private set; }
        public SteamID SID { get { return connection.User.SteamID; } }

        public enum MessageContext
        {
            Chat,
            Friend,
        }

        public DogBot()
        {
            config = Config.Load();

            logger = new Logger(LOGPATH, config.ConnectionInfo.DisplayName);
            logger.Info("Started");

            Data = new BotData();

            nameCache = new NameCache();
            nameCache.Load();

            // Create the announcer
            announcer = new Announcer(config.AnnouncementInterval);
            announcer.Announce += OnAnnounce;
            announcer.AllAnnounced += OnAllAnnouncements;

            inactivityTimer = new Timer(1000 * config.RejoinInterval);
            inactivityTimer.Elapsed += OnNoActivity;

            connection = new Connection();
            connection.LoggedOn += OnLoggedOn;
            connection.Disconnected += OnDisconnected;
            connection.ReceiveChatMessage += OnReceiveChatMessage;
            connection.ReceiveFriendMessage += OnReceiveFriendMessage;
            connection.Connect(config.ConnectionInfo);
        }

        void OnDisconnected(object sender, EventArgs e)
        {
            logger.Warning("Disconnected from Steam.");
            connection.Friends.LeaveChat(chatId);
            announcer.Stop();
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

                    logger.Info("Announcements remaining for {0}: {1}", DateTime.Now.Day.ToString(), announcer.AnnouncementsRemaining);

                    // Start the inactivity timer.
                    inactivityTimer.Start();
                }
            }
            else
            {
                logger.Error("Could not connect to chat room as the chat room ID is invalid.");
            }
        }

        void OnNoActivity(object sender, ElapsedEventArgs e)
        {
            logger.Info("Rejoining chat due to inactivity");
            connection.Friends.LeaveChat(chatId);
            connection.Friends.JoinChat(chatId);
        }

        void OnAnnounce(object sender, EventArgs e)
        {
            // Post DoTD
            if (Data.HasDog)
            {
                logger.Info("Posting announcement...");
                HandleMessage(MessageContext.Chat, SID, CommandRegistry.Dotd);
            }
        }

        void OnAllAnnouncements(object sender, EventArgs e)
        {
            logger.Info("All announcements for current dog shown.");

            Data.MoveToNextDog();

            if (Data.HasDog)
            {
                logger.Info("Moving to next dog in queue...");
            }
            else
            {
                logger.Info("Nothing to announce.");
            }
        }

        void OnReceiveChatMessage(object sender, SteamFriends.ChatMsgCallback callback)
        {
            inactivityTimer.Stop();
            inactivityTimer.Start();
            HandleMessage(MessageContext.Chat, callback.ChatterID, callback.Message);
        }

        void OnReceiveFriendMessage(object sender, SteamFriends.FriendMsgCallback callback)
        {
            HandleMessage(MessageContext.Friend, callback.Sender, callback.Message);
        }

        void HandleMessage(MessageContext context, SteamID caller, string message)
        {
            // Process the received message and pass in the current Bot's data.
            var handler = new MessageHandler(this, caller, message);

            if (handler.Record != null)
            {
                // Log info about the execution of the command.
                if (handler.Record.Executer.IsValid)
                {
                    var steamName = connection.Friends.GetFriendPersonaName(handler.Record.Executer);
                    logger.Info("Command Execution: From {0}: '{1}' by {2}. Arguments: {3}", context.ToString(), handler.Record.Command, steamName, handler.Record.Args);
                }

                // Echo the result if there is one.
                if (!string.IsNullOrEmpty(handler.Record.Result.FeedbackMessage))
                {
                    if (context == MessageContext.Chat)
                    {
                        SayToChat(chatId, handler.Record.Result.FeedbackMessage);
                    }
                    else
                    {
                        SayToFriend(caller, handler.Record.Result.FeedbackMessage);
                    }
                }

                if (!string.IsNullOrEmpty(handler.Record.Result.PrivateMessage))
                    SayToFriend(caller, handler.Record.Result.PrivateMessage);

                // Log a log message if there is one.
                if (!string.IsNullOrEmpty(handler.Record.Result.LogMessage))
                    logger.Info(handler.Record.Result.LogMessage);
            }
        }

        void SayToChat(SteamID chatId, string message)
        {
            if (!muted)
            {
                connection.Friends.SendChatRoomMessage(chatId, EChatEntryType.ChatMsg, message);
                logger.Info("@Chat: {0}", message);
            }
        }

        void SayToFriend(SteamID friend, string message)
        {
            connection.Friends.SendChatMessage(friend, EChatEntryType.ChatMsg, message);
            logger.Info("@{0}: {1}", connection.Friends.GetFriendPersonaName(friend), message);
        }

        #region Helpers
        public void PopulateNameCache()
        {
            Data.Queue.ForEach(x => GetFriendName(x.Setter));
        }

        public string GetFriendName(SteamID id)
        {
            var name = connection.Friends.GetFriendPersonaName(id);

            if (name != "[unknown]")
                nameCache.Store(id, name);

            return nameCache.Retrieve(id);
        }

        public bool IsAdmin(SteamID id)
        {
            return config.Admins != null ? config.Admins.Contains(id.ToString()) : false;
        }

        public bool IsUser(SteamID id)
        {
            return config.Users != null ? config.Users.Contains(id.ToString()) : false;
        }

        public void Mute()
        {
            SayToChat(chatId, Strings.Muted);
            muted = true;
        }

        public void Unmute()
        {
            muted = false;
            SayToChat(chatId, Strings.Unmuted);
        }
        #endregion
    }
}
