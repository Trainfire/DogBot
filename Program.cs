using Core;
using Modules.DogOfTheDay;

namespace DogBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var bot = new Bot();
            bot.RegisterModule<DogOfTheDay>();
            bot.Start();

            while (true) { }
        }
    }
}
