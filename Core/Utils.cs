using SteamKit2;

namespace Core
{
    public class ConnectionUtils
    {
        readonly Bot bot;
        readonly Connection connection;

        public bool Connected { get { return connection.Connected; } }
        public SteamID SID { get { return connection.User.SteamID; } }
        public CallbackManager CallbackManager { get { return connection.Manager; } }
        public SteamFriends Friends { get { return connection.Friends; } }
        public SteamID CurrentChatRoomID { get { return connection.ChatRoomID; } }

        public ConnectionUtils(Bot bot, Connection connection)
        {
            this.bot = bot;
            this.connection = connection;
        }

        public string GetChatRoomName(SteamID id)
        {
            return Friends.GetClanName(id);
        }

        public void SayToChat(SteamID chatId, string message)
        {
            if (chatId == null)
            {
                bot.Logger.Warning("Cannot send message to chat as the provided SteamID is either null or invalid");
                return;
            }

            if (string.IsNullOrEmpty(message))
            {
                bot.Logger.Warning("Cannot send message to chat as the message is null or empty");
                return;
            }

            Friends.SendChatRoomMessage(chatId, EChatEntryType.ChatMsg, message);
            bot.Logger.Info("@Chat: {0}", message);
        }

        public void SayToFriend(SteamID friend, string message)
        {
            if (friend == null)
            {
                bot.Logger.Warning("Cannot send message to friend as the provided SteamID is either null or invalid");
                return;
            }

            if (string.IsNullOrEmpty(message))
            {
                bot.Logger.Warning("Cannot send message to friend as the message is null or empty");
                return;
            }

            Friends.SendChatMessage(friend, EChatEntryType.ChatMsg, message);
            bot.Logger.Info("@{0}: {1}", bot.GetFriendName(friend), message);
        }
    }

    public class UserUtils
    {
        readonly Config config;

        public UserUtils(Config config)
        {
            this.config = config;
        }

        public bool Add(string steamID)
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

        public bool Remove(string steamID)
        {
            if (config.Data.Users.Contains(steamID))
            {
                config.Data.Users.Remove(steamID);
                config.Save();
                return true;
            }
            return false;
        }

        public bool IsAdmin(SteamID id)
        {
            return config.Data.Admins != null ? config.Data.Admins.Contains(id.ToString()) : false;
        }

        public bool IsUser(SteamID id)
        {
            return config.Data.Users != null ? config.Data.Users.Contains(id.ToString()) : false;
        }
    }
}