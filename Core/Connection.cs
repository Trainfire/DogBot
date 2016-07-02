using System;
using System.IO;
using SteamKit2;
using SteamKit2.Internal;
using System.Threading;
using System.Security.Cryptography;
using System.Net;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Core
{
    public interface IConnectionHandler
    {
        void OnDisconnect();
        void OnLoggedIn();
        void OnLoggedOut();
        void OnReceiveChatMessage(SteamFriends.ChatMsgCallback caller);
        void OnReceiveFriendMessage(SteamFriends.FriendMsgCallback caller);
    }

    /// <summary>
    /// Handles the connection to Steam via SteamKit2.
    /// </summary>
    public class Connection
    {
        ServerCache serverCache;
        SteamClient steamClient;
        CallbackManager manager;
        bool isRunning;
        ConnectionInfo connectionInfo;
        IConnectionHandler handler;
        //string user, pass, displayName;
        string authCode, twoFactorAuth;
        readonly Logger logger;

        public SteamUser User { get; private set; }
        public SteamFriends Friends { get; private set; }

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

        public Connection(IConnectionHandler handler, string logPath)
        {
            this.handler = handler;

            logger = new Logger(logPath, "SteamKit");

            // create our steamclient instance
            steamClient = new SteamClient();
            // create the callback manager which will route callbacks to function calls
            manager = new CallbackManager(steamClient);

            // get the steamuser handler, which is used for logging on after successfully connecting
            User = steamClient.GetHandler<SteamUser>();
            Friends = steamClient.GetHandler<SteamFriends>();

            // register a few callbacks we're interested in
            // these are registered upon creation to a callback manager, which will then route the callbacks
            // to the functions specified
            manager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
            manager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);

            manager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
            manager.Subscribe<SteamUser.LoggedOffCallback>(OnLoggedOff);

            // this callback is triggered when the steam servers wish for the client to store the sentry file
            manager.Subscribe<SteamUser.UpdateMachineAuthCallback>(OnMachineAuth);

            // Create then load cached server data.
            // NOTE: Currently, a server cache must be included in the bin directory because it's not possible to force SteamKit to refresh it's internal server list when running on Mono because reasons?
            serverCache = new ServerCache();
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

            if (!serverCache.CacheExists)
            {
                logger.Error("Cannot connect as the server cache failed to load! Make sure servers.bin exists in the root directory.");
                return;
            }

            isRunning = true;

            logger.Info("Connecting to Steam...");

            // initiate the connection
            steamClient.Connect();

            // create our callback handling loop
            while (isRunning)
            {
                // in order for the callbacks to get routed, they need to be handled by the manager
                manager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
            }
        }

        public void Disconnect()
        {
            if (isRunning)
            {
                logger.Info("Disconnected");
                steamClient.Disconnect();
            }
        }

        void Reconnect()
        {
            isRunning = false;

            logger.Info("Reconnecting in " + connectionInfo.ReconnectInterval + " seconds...");

            Thread.Sleep(TimeSpan.FromSeconds(connectionInfo.ReconnectInterval));

            Connect(connectionInfo);
        }

        void OnConnected(SteamClient.ConnectedCallback callback)
        {
            if (callback.Result != EResult.OK)
            {
                logger.Warning("Unable to connect to Steam: {0}", callback.Result);
                Reconnect();
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
            logger.Warning("Disconnected from Steam...");

            handler.OnDisconnect();

            isRunning = false;

            if (!callback.UserInitiated)
                Reconnect();
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
                Reconnect();
                return;
            }

            logger.Info("Successfully logged on!");

            // Automatically set the bot online.
            Friends.SetPersonaName(connectionInfo.DisplayName);
            Friends.SetPersonaState(EPersonaState.Online);

            // Subscribe to all Friend related callbacks here.
            manager.Subscribe<SteamFriends.ChatMsgCallback>(OnChatMessage);
            manager.Subscribe<SteamFriends.FriendMsgCallback>(OnFriendMessage);

            handler.OnLoggedIn();
        }

        void OnLoggedOff(SteamUser.LoggedOffCallback callback)
        {
            logger.Info("Logged off of Steam: {0}", callback.Result);
            handler.OnLoggedOut();
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

        void OnChatMessage(SteamFriends.ChatMsgCallback callback)
        {
            handler.OnReceiveChatMessage(callback);
        }

        void OnFriendMessage(SteamFriends.FriendMsgCallback callback)
        {
            handler.OnReceiveFriendMessage(callback);
        }
    }
}
