using System;
using System.Timers;
using System.Collections.Generic;
using SteamKit2;

namespace Core
{
    public abstract class Bot
    {
        public event EventHandler<bool> OnMute;

        readonly Connection connection;
        readonly Config config;
        readonly Timer inactivityTimer;
        readonly Logger logger;
        readonly NameCache nameCache;
        readonly CommandRegistry commandRegistry;

        Strings strings;
        SteamID chatId;
        bool muted;

        public SteamID SID { get { return connection.User.SteamID; } }
        protected Logger Logger { get; private set; }
        protected CommandRegistry CommandRegistry { get; private set; }

        public List<Command> Commands { get { return CommandRegistry.Commands; } }
        public string Token { get { return CommandRegistry.Token; } }
        public string LogPath { get { return config.Data.ConnectionInfo.DisplayName + ".bin"; } }
        public Strings CoreStrings { get { return strings; } }

        public enum MessageContext
        {
            Chat,
            Friend,
        }

        public Bot()
        {
            config = new Config();

            commandRegistry = new CommandRegistry(config.Data.Token, config.Data.CommandPrefix);

            logger = new Logger(LogPath, config.Data.ConnectionInfo.DisplayName);
            logger.Info("Started");

            nameCache = new NameCache();

            strings = GetStrings();

            inactivityTimer = new Timer(1000 * config.Data.RejoinInterval);
            inactivityTimer.Elapsed += OnNoActivity;

            connection = new Connection(LogPath);
            connection.LoggedOn += OnLoggedOn;
            connection.Disconnected += OnDisconnected;
            connection.ReceiveChatMessage += OnReceiveChatMessage;
            connection.ReceiveFriendMessage += OnReceiveFriendMessage;
            connection.Connect(config.Data.ConnectionInfo);
        }

        protected virtual Strings GetStrings()
        {
            return new Strings();
        }

        protected virtual void OnJoinChat() { }

        protected virtual void OnDisconnected(object sender, EventArgs e)
        {
            logger.Warning("Disconnected from Steam.");
            connection.Friends.LeaveChat(chatId);
        }

        void OnLoggedOn(object sender, EventArgs e)
        {
            logger.Info("Logged on");

            // Attempt to join chat.
            if (!string.IsNullOrEmpty(config.Data.ChatRoomId))
            {
                ulong chatRoomId = 0;
                ulong.TryParse(config.Data.ChatRoomId, out chatRoomId);

                if (chatRoomId == 0)
                {
                    logger.Error("{0} is an invalid chat room ID", config.Data.ChatRoomId);
                }
                else
                {
                    connection.Friends.JoinChat(chatRoomId);
                    chatId = new SteamID(chatRoomId);

                    // Start the inactivity timer.
                    inactivityTimer.Start();

                    OnJoinChat();
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

        protected void HandleMessage(MessageContext context, SteamID caller, string message)
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
        [Obsolete]
        public void PopulateNameCache()
        {
            // TODO.
        }

        public virtual string NoPermission { get { return "*bark!* You do not have permission to do that!"; } }
        public virtual string Muted { get { return "*muted*"; } }
        public virtual string Unmuted { get { return "*bark!*"; } }

        public string GetFriendName(SteamID id)
        {
            CacheName(id);
            return nameCache.Retrieve(id);
        }

        public void CacheName(SteamID id)
        {
            var name = connection.Friends.GetFriendPersonaName(id);

            if (name != "[unknown]")
                nameCache.Store(id, name);
        }

        public bool IsAdmin(SteamID id)
        {
            return config.Data.Admins != null ? config.Data.Admins.Contains(id.ToString()) : false;
        }

        public bool IsUser(SteamID id)
        {
            return config.Data.Users != null ? config.Data.Users.Contains(id.ToString()) : false;
        }

        public void Mute()
        {
            muted = true;
            if (OnMute != null)
                OnMute(this, muted);
        }

        public void Unmute()
        {
            muted = false;
            if (OnMute != null)
                OnMute(this, muted);
        }
        #endregion
    }
}
