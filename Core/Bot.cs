using System;
using System.Collections.Generic;
using SteamKit2;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using Modules.BotManager;

namespace Core
{
    public sealed class Bot
    {
        readonly Config config;
        readonly NameCache nameCache;
        readonly List<ILogOnCallbackHandler> logOnListeners;
        readonly List<ILogOffCallbackHandler> logOffListeners;

        #region Utils
        public Logger Logger { get; private set; }
        public CommandListener CommandListener { get; private set; }
        public ModuleManager Modules { get; private set; }
        public ConnectionUtils Connection { get; private set; }
        public UserUtils Users { get; private set; }
        #endregion

        public SteamID CurrentChatRoomID { get; private set; }

        public Bot(Config config, Connection connection, Logger logger)
        {
            this.config = config;

            nameCache = new NameCache();

            Users = new UserUtils(config);
            Connection = new ConnectionUtils(this, connection);
            Modules = new ModuleManager(this);
            CommandListener = new CommandListener(this);
            Logger = logger;

            logOnListeners = new List<ILogOnCallbackHandler>();
            logOffListeners = new List<ILogOffCallbackHandler>();

            // Subscribe to callbacks here.
            connection.Manager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);

            // Add base module(s).
            Modules.Add<BotManager>();

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
                    logger.Error("Failed to load module '{0}'. Either the path is invalid or the module does not exist.", moduleName);
                }
                else
                {
                    var instance = Activator.CreateInstance(type) as Module;
                    Modules.Add(instance);
                }
            });
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
            var name = Connection.Friends.GetFriendPersonaName(id);

            if (name != "[unknown]")
                nameCache.Store(id, name);
        }

        public void CacheNames(List<SteamID> names)
        {
            names.ForEach(x => CacheName(x));
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

        void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {
            if (callback.Result == EResult.OK)
                logOnListeners.ForEach(x => x.OnLoggedOn());
        }
    }

    public class BotController
    {
        readonly Config config;
        readonly Connection connection;
        readonly Logger logger;

        public BotController()
        {
            config = new Config();

            string logPath = config.Data.ConnectionInfo.DisplayName + ".bin";

            connection = new Connection(logPath);

            logger = new Logger(logPath, config.Data.ConnectionInfo.DisplayName);
            logger.Info("Started");

            var bot = new Bot(config, connection, logger);
        }

        public void Start()
        {
            logger.Info("Starting...");

            // Start the connection in a seperate thread to prevent thread blocking.
            connection.Connect(config.Data.ConnectionInfo);
        }

        public void RefreshServers()
        {
            connection.RepopulateServerCache();
        }

        public void Restart()
        {
            RestartAsync();
        }

        async void RestartAsync()
        {
            connection.Disconnect();
            await Task.Delay(1000);
            connection.Connect(config.Data.ConnectionInfo);
        }

        public void Stop()
        {
            logger.Info("Stopping...");
            connection.Disconnect();
        }
    }
}