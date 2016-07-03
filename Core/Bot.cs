using System;
using System.Timers;
using System.Collections.Generic;
using SteamKit2;
using System.Threading;

namespace Core
{
    public sealed class Bot
    {
        readonly Config config;
        readonly Logger logger;
        readonly NameCache nameCache;
        readonly Connection connection;

        List<Module> modules;
        List<ILogOnCallbackHandler> logOnListeners;
        List<ILogOffCallbackHandler> logOffListeners;
        Thread connectionThread;

        public SteamID SID { get { return connection.User.SteamID; } }
        public string LogPath { get { return config.Data.ConnectionInfo.DisplayName + ".bin"; } }

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

            logger = new Logger(LogPath, config.Data.ConnectionInfo.DisplayName);
            logger.Info("Started");

            // Move into Module?
            nameCache = new NameCache();

            modules = new List<Module>();
        }

        public void Start()
        {
            logger.Info("Starting...");

            // Start the connection in a seperate thread to prevent thread blocking.
            connectionThread = new Thread(() => connection.Connect(config.Data.ConnectionInfo));
            connectionThread.Start();
        }

        public T AddModule<T>() where T : Module
        {
            logger.Info("Registering module '{0}'", typeof(T).Name);
            var instance = Activator.CreateInstance<T>();
            modules.Add(instance);
            instance.Initialize(this);
            return instance;
        }

        public T GetModule<T>() where T : Module
        {
            var module = modules.Find(x => x.GetType() == typeof(T));
            if (module != null)
                return module as T;
            return null;
        }

        public T GetOrAddModule<T>() where T : Module
        {
            var module = GetModule<T>();
            if (module == null)
                module = AddModule<T>();
            return module;
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

        public void SayToChat(SteamID chatId, string message)
        {
            Friends.SendChatRoomMessage(chatId, EChatEntryType.ChatMsg, message);
            logger.Info("@Chat: {0}", message);
        }

        public void SayToFriend(SteamID friend, string message)
        {
            Friends.SendChatMessage(friend, EChatEntryType.ChatMsg, message);
            logger.Info("@{0}: {1}", GetFriendName(friend), message);
        }
        #endregion
    }
}
