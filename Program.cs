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
                if (Console.ReadLine() == "stop")
                    bot.Stop();

                if (Console.ReadLine() == "start")
                    bot.Start();

                if (Console.ReadLine() == "quit")
                {
                    bot.Stop();
                    isRunning = false;
                }
            }
        }
    }
}
