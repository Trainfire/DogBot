using System;
using System.IO;
using SteamKit2;
using System.Threading.Tasks;
using System.Timers;
using System.Security.Cryptography;

namespace Core
{
    public interface ILogOnCallbackHandler
    {
        void OnLoggedOn();
    }

    public interface ILogOffCallbackHandler
    {
        void OnLoggedOff();
    }

    /// <summary>
    /// Handles the connection to Steam via SteamKit2.
    /// </summary>
    public class Connection
    {
        ServerCache serverCache;
        SteamClient steamClient;
        bool isRunning;
        ConnectionInfo connectionInfo;
        string authCode, twoFactorAuth;

        readonly Logger logger;
        readonly System.Timers.Timer timer;

        public SteamUser User { get; private set; }
        public SteamFriends Friends { get; private set; }
        public CallbackManager Manager { get; private set; }
        public bool Connected { get { return isRunning; } }
        public SteamID ChatRoomID { get; private set; }

        public class ConnectionInfo
        {
            public string User { get; set; }
            public string Pass { get; set; }
            public string DisplayName { get; set; }
            public float ReconnectInterval { get; set; }

            public ConnectionInfo()
            {
                User = "User";
                Pass = "Pass";
                DisplayName = "Unnamed";
                ReconnectInterval = 300;
            }
        }

        public Connection(string logPath)
        {
            logger = new Logger(logPath, "SteamKit");

            steamClient = new SteamClient();

            // Create the tick timer that will handle any callbacks from Steam.
            timer = new System.Timers.Timer(100);
            timer.Elapsed += Tick;
            timer.Start();

            // create the callback manager which will route callbacks to function calls
            Manager = new CallbackManager(steamClient);

            // get the steamuser handler, which is used for logging on after successfully connecting
            User = steamClient.GetHandler<SteamUser>();
            Friends = steamClient.GetHandler<SteamFriends>();

            // register a few callbacks we're interested in
            // these are registered upon creation to a callback Manager, which will then route the callbacks
            // to the functions specified
            Manager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
            Manager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);
            Manager.Subscribe<SteamFriends.ChatEnterCallback>(OnJoinChat);
            Manager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
            Manager.Subscribe<SteamUser.LoggedOffCallback>(OnLoggedOff);

            // this callback is triggered when the steam servers wish for the client to store the sentry file
            Manager.Subscribe<SteamUser.UpdateMachineAuthCallback>(OnMachineAuth);

            // Create then load cached server data.
            // NOTE: Currently, a server cache must be included in the bin directory because it's not possible to force SteamKit to refresh it's internal server list when running on Mono because reasons?
            serverCache = new ServerCache(logger);
            serverCache.Load();
        }

        public void Connect(ConnectionInfo connectionInfo)
        {
            this.connectionInfo = connectionInfo;

            if (string.IsNullOrEmpty(connectionInfo.User) || string.IsNullOrEmpty(connectionInfo.Pass))
            {
                logger.Error("Will not connect to Steam as User or Password is null or empty.");
                return;
            }

            isRunning = true;

            logger.Info("Connecting to Steam...");

            // initiate the connection
            steamClient.Connect();
        }

        void Tick(object sender, ElapsedEventArgs e)
        {
            if (isRunning)
                Manager.RunCallbacks();
        }

        public void Disconnect()
        {
            if (isRunning)
            {
                logger.Info("Disconnected");
                steamClient.Disconnect();
            }
        }

        async void ReconnectAsync()
        {
            isRunning = false;
            logger.Info("Reconnecting in " + connectionInfo.ReconnectInterval + " seconds...");
            await Task.Delay(TimeSpan.FromSeconds(connectionInfo.ReconnectInterval));
            Connect(connectionInfo);
        }

        void OnConnected(SteamClient.ConnectedCallback callback)
        {
            if (callback.Result != EResult.OK)
            {
                logger.Warning("Unable to connect to Steam: {0}", callback.Result);
                ReconnectAsync();
                return;
            }

            logger.Info("Connected to Steam! Logging in '{0}'...", connectionInfo.User);

            byte[] sentryHash = null;
            if (File.Exists("sentry.bin"))
            {
                // if we have a saved sentry file, read and sha-1 hash it
                byte[] sentryFile = File.ReadAllBytes("sentry.bin");
                sentryHash = CryptoHelper.SHAHash(sentryFile);
            }

            User.LogOn(new SteamUser.LogOnDetails
            {
                Username = connectionInfo.User,
                Password = connectionInfo.Pass,

                // in this sample, we pass in an additional authcode
                // this value will be null (which is the default) for our first logon attempt
                AuthCode = authCode,

                // if the account is using 2-factor auth, we'll provide the two factor code instead
                // this will also be null on our first logon attempt
                TwoFactorCode = twoFactorAuth,

                // our subsequent logons use the hash of the sentry file as proof of ownership of the file
                // this will also be null for our first (no authcode) and second (authcode only) logon attempts
                SentryFileHash = sentryHash,
            });
        }

        void OnDisconnected(SteamClient.DisconnectedCallback callback)
        {
            // after recieving an AccountLogonDenied, we'll be disconnected from steam
            // so after we read an authcode from the user, we need to reconnect to begin the logon flow again

            isRunning = false;

            if (callback.UserInitiated)
            {
                logger.Warning("Disconnected from Steam by the user.");
            }
            else
            {
                logger.Warning("Disconnected from Steam...");
                ReconnectAsync();
            }
        }

        void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {
            bool isSteamGuard = callback.Result == EResult.AccountLogonDenied;
            bool is2FA = callback.Result == EResult.AccountLoginDeniedNeedTwoFactor;

            if (isSteamGuard || is2FA)
            {
                logger.Warning("This account is SteamGuard protected!");

                if (is2FA)
                {
                    logger.Info("Please enter your 2 factor auth code from your authenticator app: ");
                    twoFactorAuth = Console.ReadLine();
                }
                else
                {
                    logger.Info("Please enter the auth code sent to the email at {0}: ", callback.EmailDomain);
                    authCode = Console.ReadLine();
                }

                return;
            }

            if (callback.Result != EResult.OK)
            {
                logger.Error("Unable to logon to Steam: {0} / {1}", callback.Result, callback.ExtendedResult);
                ReconnectAsync();
                return;
            }

            logger.Info("Successfully logged on!");

            // Automatically set the bot online.
            Friends.SetPersonaName(connectionInfo.DisplayName);
            Friends.SetPersonaState(EPersonaState.Online);
        }

        void OnLoggedOff(SteamUser.LoggedOffCallback callback)
        {
            logger.Info("Logged off of Steam: {0}", callback.Result);
        }

        void OnMachineAuth(SteamUser.UpdateMachineAuthCallback callback)
        {
            logger.Info("Updating sentryfile...");

            // write out our sentry file
            // ideally we'd want to write to the filename specified in the callback
            // but then this sample would require more code to find the correct sentry file to read during logon
            // for the sake of simplicity, we'll just use "sentry.bin"

            int fileSize;
            byte[] sentryHash;
            using (var fs = File.Open("sentry.bin", FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                fs.Seek(callback.Offset, SeekOrigin.Begin);
                fs.Write(callback.Data, 0, callback.BytesToWrite);
                fileSize = (int)fs.Length;

                fs.Seek(0, SeekOrigin.Begin);
                using (var sha = new SHA1CryptoServiceProvider())
                {
                    sentryHash = sha.ComputeHash(fs);
                }
            }

            // inform the steam servers that we're accepting this sentry file
            User.SendMachineAuthResponse(new SteamUser.MachineAuthDetails
            {
                JobID = callback.JobID,

                FileName = callback.FileName,

                BytesWritten = callback.BytesToWrite,
                FileSize = fileSize,
                Offset = callback.Offset,

                Result = EResult.OK,
                LastError = 0,

                OneTimePassword = callback.OneTimePassword,

                SentryFileHash = sentryHash,
            });

            logger.Info("Done!");
        }

        void OnJoinChat(SteamFriends.ChatEnterCallback callback)
        {
            ChatRoomID = callback.ChatID;
        }

        public void RepopulateServerCache()
        {
            serverCache.Refresh();
        }
    }
}
