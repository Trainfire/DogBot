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
                    commands.Add(new Command<SetDogOfTheDay>(DOTDSET)
                    {
                        AdminOnly = true,
                        HelpArgs = new List<string>()
                        {
                            "URL",
                            "Comment (Optional)"
                        }
                    });
                    commands.Add(new Command<Help>(HELP));
                }
                return commands.ToList();
            }
        }

        const string DOTD = "dotd";
        const string DOTDSET = "dotdset";
        const string HELP = "dotdhelp";

        public const string COMMAND_TOKEN = "!";

        public static string Dotd { get { return Format(DOTD); } }

        public static string DotdSet(string url, string comment)
        {
            return string.Format("{0} {1} {2}", Format(DOTDSET), url, comment);
        }

        static string Format(string command)
        {
            return string.Format("{0}{1}", COMMAND_TOKEN, command);
        }
    }
}
