using System;
using Core;
using Modules.ChatJoiner;
using Modules.CommandHandler;
using Modules.DogOfTheDay;
using Modules.Twitter;

namespace DogBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var bot = new Bot();
            bot.Start();

            bool isRunning = true;
            while (isRunning)
            {
                var input = Console.ReadLine();

                if (input == "stop")
                    bot.Stop();

                if (input == "start")
                    bot.Start();

                if (input == "quit")
                {
                    isRunning = false;
                }
            }
        }
    }
}
