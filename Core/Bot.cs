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

        #region Utils
        public Names Names { get; private set; }
        public Logger Logger { get; private set; }
        public CommandListener CommandListener { get; private set; }
        public ModuleManager Modules { get; private set; }
        public ConnectionUtils Connection { get; private set; }
        public UserUtils Users { get; private set; }
        public LoginStateHandler LogStateHandler { get; private set; }
        #endregion

        public SteamID CurrentChatRoomID { get { return Connection.CurrentChatRoomID; } }

        public Bot(Config config, Connection connection, Logger logger)
        {
            this.config = config;

            var nameStorage = new NameCache();
            Names = new Names(connection.Friends, nameStorage);

            Users = new UserUtils(config);
            Connection = new ConnectionUtils(this, connection);
            Modules = new ModuleManager(this);
            CommandListener = new CommandListener(this);
            LogStateHandler = new LoginStateHandler(connection.Manager);
            Logger = logger;

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
    }
}