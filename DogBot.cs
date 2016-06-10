using SteamKit2;
using System;
using System.Threading.Tasks;

namespace DogBot
{
    class DogBot
    {
        readonly Connection connection;
        readonly ConfigData config;

        public DogBot()
        {
            config = Config.Load();

            connection = new Connection();
            connection.LoggedOn += Connection_LoggedOn;
            connection.RecieveMessage += OnReceiveMessage;
            connection.Connect(config.User, config.Pass);
        }

        void Connection_LoggedOn(object sender, EventArgs e)
        {
            Log("Logged on");

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
                }
            }
            else
            {
                Log("Could not connect to chat room as the chat room ID is invalid.");
            }
        }

        void OnReceiveMessage(object sender, SteamFriends.ChatMsgCallback e)
        {
            Console.WriteLine(e.Message);
        }

        void Log(string message, params object[] args)
        {
            Console.WriteLine("[DogBot] " + string.Format(message, args));
        }
    }
}
