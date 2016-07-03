using System;
using Core;
using Modules.ChatJoiner;
using Modules.CommandHandler;
using Modules.DogOfTheDay;

namespace DogBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var bot = new Bot();
            bot.AddModule<ChatJoiner>();
            bot.AddModule<CommandListener>();
            bot.AddModule<DogOfTheDay>();
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
