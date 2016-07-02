using System;
using Core;
using Modules.DogOfTheDay;
using Modules.ChatJoiner;

namespace DogBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var bot = new Bot();
            bot.RegisterModule<DogOfTheDay>();
            bot.RegisterModule<ChatJoiner>();
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
