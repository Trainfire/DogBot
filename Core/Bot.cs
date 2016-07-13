using System;
using System.Collections.Generic;
using SteamKit2;
using System.Threading;
using System.Linq;

namespace Core
{
    public sealed class Bot
    {
        readonly Config config;
        readonly NameCache nameCache;
        readonly Connection connection;
        readonly CoreCommandHandler commandHandler;

        List<Module> modules;
        List<ILogOnCallbackHandler> logOnListeners;
        List<ILogOffCallbackHandler> logOffListeners;
        Thread connectionThread;

        public Logger Logger { get; private set; }
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

            Logger = new Logger(LogPath, config.Data.ConnectionInfo.DisplayName);
            Logger.Info("Started");

            // Move into Module?
            nameCache = new NameCache();
            modules = new List<Module>();

            // Load modules dynamically.
            config.Data.Modules.ForEach(moduleName =>
            {
                // Modules must be in the namespace 'Modules.ModuleName'
                // A module must have contain a class of the same name that derives from Module.
                // ---
                // For example, the Twitter module has the class 'Twitter' that derives from Module
                // in the namespace 'Modules.Twitter'.
                string moduleClassName = string.Format("Modules.{0}.{0}", moduleName);

                var type = Type.GetType(moduleClassName);
                if (type == null)
                {
                    Logger.Error("Failed to load module '{0}'. Either the path is invalid or the module does not exist.", moduleName);
                }
                else
                {
                    var instance = Activator.CreateInstance(type) as Module;
                    AddModule(instance);
                }
            });

            commandHandler = new CoreCommandHandler(this);
        }

        public void Start()
        {
            Logger.Info("Starting...");

            // Start the connection in a seperate thread to prevent thread blocking.
            connectionThread = new Thread(() => connection.Connect(config.Data.ConnectionInfo));
            connectionThread.Start();
        }

        public T AddModule<T>() where T : Module
        {
            var instance = Activator.CreateInstance<T>();
            AddModule(instance);
            return instance;
        }

        void AddModule(Module module)
        {
            if (modules.Any(x => x.GetType() == module.GetType()))
            {
                Logger.Error("Module of type '{0}' has already been added. Only one module of each type is allowed.", module.GetType());
            }
            else
            {
                Logger.Info("Registering module '{0}'", module.GetType().Name);
                modules.Add(module);
                module.Initialize(this);
            }
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
            Logger.Info("Stopping...");
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
        public bool AddUser(string steamID)
        {
            var parsedSID = new SteamID(steamID);

            if (!parsedSID.IsValid)
                return false;

            if (config.Data.Users.Contains(parsedSID.Render()))
            {
                return false;
            }
            else
            {
                config.Data.Users.Add(steamID);
                config.Save();
            }

            return true;
        }

        public bool RemoveUser(string steamID)
        {
            if (config.Data.Users.Contains(steamID))
            {
                config.Data.Users.Remove(steamID);
                config.Save();
                return true;
            }
            return false;
        }

        public string GetChatRoomName(SteamID id)
        {
            return connection.Friends.GetClanName(id);
        }

        public string GetFriendName(string steamID3)
        {
            return GetFriendName(new SteamID(steamID3));
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

        public void CacheNames(List<SteamID> names)
        {
            names.ForEach(x => CacheName(x));
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
            if (chatId == null)
            {
                Logger.Warning("Cannot send message to chat as the provided SteamID is either null or invalid");
                return;
            }

            if (string.IsNullOrEmpty(message))
            {
                Logger.Warning("Cannot send message to chat as the message is null or empty");
                return;
            }

            Friends.SendChatRoomMessage(chatId, EChatEntryType.ChatMsg, message);
            Logger.Info("@Chat: {0}", message);
        }

        public void SayToFriend(SteamID friend, string message)
        {
            if (friend == null)
            {
                Logger.Warning("Cannot send message to friend as the provided SteamID is either null or invalid");
                return;
            }

            if (string.IsNullOrEmpty(message))
            {
                Logger.Warning("Cannot send message to friend as the message is null or empty");
                return;
            }

            Friends.SendChatMessage(friend, EChatEntryType.ChatMsg, message);
            Logger.Info("@{0}: {1}", GetFriendName(friend), message);
        }
        #endregion
    }
}
