using System;
using SteamKit2;

namespace DogBot
{
    public class DogBot
    {
        readonly Connection connection;
        readonly ConfigData config;

        SteamID chatId;

        public BotData Data { get; private set; }

        public DogBot()
        {
            config = Config.Load();

            connection = new Connection();
            connection.LoggedOn += OnLoggedOn;
            connection.RecieveMessage += OnReceiveMessage;
            connection.Connect(config.User, config.Pass);
        }

        void OnLoggedOn(object sender, EventArgs e)
        {
            Log("Logged on");

            Data = new BotData(connection.Friends);

            connection.Friends.SetPersonaName(config.SteamName);

            if (!string.IsNullOrEmpty(config.ChatRoomId))
            {
                ulong chatRoomId = 0;
                ulong.TryParse(config.ChatRoomId, out chatRoomId);

                if (chatRoomId == 0)
                {
                    Log("{0} is an invalid chat room ID", config.ChatRoomId);
                }
                else
                {
                    connection.Friends.JoinChat(chatRoomId);
                    chatId = new SteamID(chatRoomId);
                }
            }
            else
            {
                Log("Could not connect to chat room as the chat room ID is invalid.");
            }
        }

        void OnReceiveMessage(object sender, SteamFriends.ChatMsgCallback callback)
        {
            // Process the received message and pass in the current Bot's data.
            var handler = new MessageHandler(this, callback);

            // Echo the result if there is one.
            if (handler.Result != null)
                Say(chatId, handler.Result);
        }

        void Say(SteamID chatId, string message)
        {
            connection.Friends.SendChatRoomMessage(chatId, EChatEntryType.ChatMsg, message);
        }

        void Log(string message, params object[] args)
        {
            Console.WriteLine("[DogBot] " + string.Format(message, args));
        }

        public string GetFriendName(SteamID id)
        {
            return connection.Friends.GetFriendPersonaName(id);
        }
    }
}
