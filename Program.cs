using System;
using Core;
using Core.Utils;
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

            Download();

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

        static async void Download()
        {
            var imageDownloader = new ImageDownloader();
            var data = await imageDownloader.Download("https://pbs.twimg.com/media/CmT0EXgAEmSyY.jpg");
            if (data != null)
            {
                Console.WriteLine("Success!");
            }
            else
            {
                Console.WriteLine("Failed...");
            }
        }
    }
}
