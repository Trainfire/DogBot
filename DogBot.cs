using SteamKit2;
using System;

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

        private void Connection_LoggedOn(object sender, EventArgs e)
        {
            Console.WriteLine("Logged on");

            connection.Friends.SetPersonaName(config.SteamName);
            connection.Friends.JoinChat(ulong.Parse("103582791454793610"));
        }

        private void OnReceiveMessage(object sender, SteamFriends.ChatMsgCallback e)
        {
            Console.WriteLine(e.Message);
        }
    }
}
