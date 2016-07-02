using System;
using System.Timers;
using System.Collections.Generic;
using SteamKit2;
using System.Threading;

namespace Core
{
    public sealed class Bot
    {
        public event EventHandler<bool> OnMute;
        readonly Config config;
        readonly Logger logger;
        readonly NameCache nameCache;
        readonly CommandRegistry commandRegistry;
        readonly Connection connection;

        List<Module> modules;
        List<ILogOnCallbackHandler> logOnListeners;
        List<ILogOffCallbackHandler> logOffListeners;
        Strings strings;
        SteamID currentChatID;
        bool muted;
        Thread connectionThread;

        public SteamID SID { get { return connection.User.SteamID; } }
        public Logger Logger { get; private set; }
        public List<Command> Commands { get { return commandRegistry.Commands; } }
        public string Token { get { return commandRegistry.Token; } }
        public string LogPath { get { return config.Data.ConnectionInfo.DisplayName + ".bin"; } }
        public Strings CoreStrings { get { return strings; } }
        public CallbackManager CallbackManager { get { return connection.Manager; } }

        public enum MessageContext
        {
            Chat,
            Friend,
        }

        public Bot()
        {
            config = new Config();

            logOffListeners = new List<ILogOffCallbackHandler>();
            logOnListeners = new List<ILogOnCallbackHandler>();

            connection = new Connection(LogPath);

            // Subscribe to callbacks here.
            connection.Manager.Subscribe<SteamFriends.ChatMsgCallback>(OnReceiveChatMessage);
            connection.Manager.Subscribe<SteamFriends.FriendMsgCallback>(OnReceiveFriendMessage);
            connection.Manager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);

            currentChatID = 0;

            commandRegistry = new CommandRegistry(config.Data.Token, config.Data.CommandPrefix);

            logger = new Logger(LogPath, config.Data.ConnectionInfo.DisplayName);
            logger.Info("Started");

            nameCache = new NameCache();

            modules = new List<Module>();

            strings = GetStrings();
        }

        public void Start()
        {
            logger.Info("Starting...");

            // Start the connection in a seperate thread to prevent thread blocking.
            connectionThread = new Thread(() => connection.Connect(config.Data.ConnectionInfo));
            connectionThread.Start();
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
            logger.Info("Stopping...");
            connection.Disconnect();
        }

        public void RegisterLogOnListener(ILogOnCallbackHandler handler)
        {
            logOnListeners.Add(handler);
        }

        public void UnregisterLogOnListener(ILogOnCallbackHandler handler)
        {
            if (logOnListeners.Contains(handler))
                logOnListeners.Remove(handler);
        }

        public void RegisterLogOffListener(ILogOffCallbackHandler handler)
        {
            logOffListeners.Add(handler);
        }

        public void UnregisterLogOffListener(ILogOffCallbackHandler handler)
        {
            if (logOffListeners.Contains(handler))
                logOffListeners.Remove(handler);
        }

        #region Bot Behaviours
        void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {
            if (callback.Result == EResult.OK)
                logOnListeners.ForEach(x => x.OnLoggedOn());
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
                        SayToChat(currentChatID, handler.Record.Result.FeedbackMessage);
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
        #endregion

        public string NoPermission { get { return "*bark!* You do not have permission to do that!"; } }
        public string Muted { get { return "*muted*"; } }
        public string Unmuted { get { return "*bark!*"; } }

        public Strings GetStrings() { return new Strings(); }

        #region Helpers
        [Obsolete]
        public void PopulateNameCache() { }

        public void JoinChat(SteamID chatId)
        {
            this.currentChatID = chatId;
            connection.Friends.JoinChat(chatId);
        }

        public void LeaveChat(SteamID chatId)
        {
            this.currentChatID = 0;
            connection.Friends.LeaveChat(chatId);
        }

        public string GetChatRoomName(SteamID id)
        {
            return connection.Friends.GetClanName(id);
        }

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
