using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DogBot
{
    class CommandRegistry
    {
        static List<Command> commands;
        public static List<Command> Commands
        {
            get
            {
                if (commands == null)
                {
                    commands = new List<Command>();

                    // Register commands here
                    commands.Add(new Command<GetDogOfTheDay>(DOTD));
                    commands.Add(new Command<SetDogOfTheDay>(DOTDSET));
                }
                return commands.ToList();
            }
        }

        public const string DOTD = "dotd";
        public const string DOTDSET = "dotdset";
    }
}
