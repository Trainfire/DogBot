using System.Threading.Tasks;

namespace Core
{
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
