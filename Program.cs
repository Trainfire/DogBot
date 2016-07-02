using System;
using Core;
using Modules.ChatJoiner;
using Modules.Messenger;

namespace DogBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var bot = new Bot();
            bot.AddModule<ChatJoiner>();
            bot.AddModule<Messenger>();
            bot.Start();

            while (true)
            {
                if (Console.ReadLine() == "stop")
                    bot.Stop();

                if (Console.ReadLine() == "start")
                    bot.Start();
            }
        }
    }
}
