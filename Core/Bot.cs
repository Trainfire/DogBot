using System;
using System.Timers;
using System.Collections.Generic;
using SteamKit2;

namespace Core
{
    public interface IBot : IConnectionHandler
    {
        void OnJoinChat(SteamID chatroomID);
        void OnLeaveChat(SteamID chatroomID);
        void OnNoActivity();
    }

    public sealed class Bot : IConnectionHandler
    {
        public event EventHandler<bool> OnMute;

        readonly Connection connection;
        readonly Config config;
        readonly Timer inactivityTimer;
        readonly Logger logger;
        readonly NameCache nameCache;
        readonly CommandRegistry commandRegistry;

        List<Module> modules;
        Strings strings;
        SteamID chatId;
        bool muted;

        public SteamID SID { get { return connection.User.SteamID; } }
        public Logger Logger { get; private set; }

        public List<Command> Commands { get { return commandRegistry.Commands; } }
        public string Token { get { return commandRegistry.Token; } }
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

            chatId = 0;

            commandRegistry = new CommandRegistry(config.Data.Token, config.Data.CommandPrefix);

            logger = new Logger(LogPath, config.Data.ConnectionInfo.DisplayName);
            logger.Info("Started");

            nameCache = new NameCache();

            modules = new List<Module>();

            strings = GetStrings();

            inactivityTimer = new Timer(1000 * config.Data.RejoinInterval);
            inactivityTimer.Elapsed += OnNoActivity;

            connection = new Connection(this, LogPath);
            //connection.LoggedOn += OnLogin;
            //connection.Disconnected += OnDisconnect;
            //connection.ReceiveChatMessage += OnReceiveChatMessage;
            //connection.ReceiveFriendMessage += OnReceiveFriendMessage;
            connection.Connect(config.Data.ConnectionInfo);
        }

        public void Start()
        {
            connection.Connect(config.Data.ConnectionInfo);
        }

        public void RegisterModule<T>() where T : Module
        {
            logger.Info("Registering module '{0}'", typeof(T).Name);
            var instance = Activator.CreateInstance<T>();
            instance.Commands.ForEach(x => commandRegistry.AddCommand(x));
            modules.Add(instance);
            instance.Initialize(this);
        }

        public T GetModule<T>() where T : Module
        {
            var module = modules.Find(x => x.GetType() == typeof(T));
            if (module != null)
                return module as T;
            return null;
        }

        public void Stop()
        {
            connection.Disconnect();
        }

        #region Connection Handlers
        void IConnectionHandler.OnDisconnect()
        {
            logger.Warning("Disconnected from Steam.");

            if (chatId != 0)
                connection.Friends.LeaveChat(chatId);

            modules.ForEach(x => x.OnDisconnect());
        }

        void IConnectionHandler.OnLoggedIn()
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

                    OnJoinChat(chatId);
                }
            }
            else
            {
                logger.Error("Could not connect to chat room as the chat room ID is invalid.");
            }
        }

        void IConnectionHandler.OnLoggedOut()
        {
            modules.ForEach(x => x.OnLoggedOut());
        }

        void IConnectionHandler.OnReceiveChatMessage(SteamFriends.ChatMsgCallback callback)
        {
            inactivityTimer.Stop();
            inactivityTimer.Start();
            HandleMessage(MessageContext.Chat, callback.ChatterID, callback.Message);
        }

        void IConnectionHandler.OnReceiveFriendMessage(SteamFriends.FriendMsgCallback callback)
        {
            HandleMessage(MessageContext.Friend, callback.Sender, callback.Message);
        }
        #endregion

        #region Bot Behaviours
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

                // Log a log message if there is one.
                if (!string.IsNullOrEmpty(handler.Record.Result.LogMessage))
                    logger.Info(handler.Record.Result.LogMessage);
            }
        }

        void OnJoinChat(SteamID chatId)
        {
            modules.ForEach(x => x.OnJoinChat(chatId));
        }

        void OnLeaveChat(SteamID chatroomID)
        {
            modules.ForEach(x => x.OnLeaveChat(chatId));
        }

        void OnNoActivity(object sender, ElapsedEventArgs e)
        {
            logger.Info("Rejoining chat due to inactivity");
            connection.Friends.LeaveChat(chatId);
            connection.Friends.JoinChat(chatId);

            modules.ForEach(x => x.OnNoActivity());
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
        #endregion

        public string NoPermission { get { return "*bark!* You do not have permission to do that!"; } }
        public string Muted { get { return "*muted*"; } }
        public string Unmuted { get { return "*bark!*"; } }

        public Strings GetStrings() { return new Strings(); }

        #region Helpers
        [Obsolete]
        public void PopulateNameCache() { }

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

        /// <summary>
        /// Processes the message internally as if it was recieved as a command from a user.
        /// </summary>
        /// <param name="message"></param>
        public void ProcessMessageInternally(MessageContext context, string message)
        {
            HandleMessage(MessageContext.Chat, SID, message);
        }
        #endregion
    }
}
