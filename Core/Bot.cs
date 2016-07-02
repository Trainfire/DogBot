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
        Thread connectionThread;

        public SteamID SID { get { return connection.User.SteamID; } }
        public Logger Logger { get; private set; }
        public List<Command> Commands { get { return commandRegistry.Commands; } }
        public string Token { get { return commandRegistry.Token; } }
        public string LogPath { get { return config.Data.ConnectionInfo.DisplayName + ".bin"; } }
        public Strings CoreStrings { get { return strings; } }

        #region Connection Wrappers
        public CallbackManager CallbackManager { get { return connection.Manager; } }
        public SteamFriends Friends { get { return connection.Friends; } }
        #endregion

        public SteamID CurrentChatRoomID { get; private set; }

        public Bot()
        {
            config = new Config();

            logOffListeners = new List<ILogOffCallbackHandler>();
            logOnListeners = new List<ILogOnCallbackHandler>();

            connection = new Connection(LogPath);

            // Subscribe to callbacks here.
            connection.Manager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
            connection.Manager.Subscribe<SteamFriends.ChatEnterCallback>(OnJoinChat);

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

        public void AddModule<T>() where T : Module
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

        #region Callbacks
        void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {
            if (callback.Result == EResult.OK)
                logOnListeners.ForEach(x => x.OnLoggedOn());
        }

        void OnJoinChat(SteamFriends.ChatEnterCallback callback)
        {
            CurrentChatRoomID = callback.ChatID;
        }
        #endregion

        public string NoPermission { get { return "*bark!* You do not have permission to do that!"; } }
        public string Muted { get { return "*muted*"; } }
        public string Unmuted { get { return "*bark!*"; } }

        public Strings GetStrings() { return new Strings(); }

        #region Helpers
        [Obsolete]
        public void PopulateNameCache() { }

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
        /// <summary>
        /// Processes the message internally as if it was recieved as a command from a user.
        /// </summary>
        /// <param name="message"></param>
        //public void ProcessMessageInternally(MessageContext context, string message)
        //{
        //    HandleMessage(MessageContext.Chat, SID, message);
        //}
        #endregion
    }
}
