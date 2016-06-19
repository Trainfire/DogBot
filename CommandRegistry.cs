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
                    commands.Add(new Command<SubmitDogOfTheDay>(DOTDSUBMIT)
                    {
                        AdminOnly = true,
                        HelpArgs = new List<string>()
                        {
                            "URL",
                            "Comment (Optional)"
                        }
                    });
                    commands.Add(new Command<Help>(HELP));
                    commands.Add(new Command<Stats>(STATS));
                    commands.Add(new Command<Mute>(MUTE)
                    {
                        AdminOnly = true,
                    });
                    commands.Add(new Command<Unmute>(UNMUTE)
                    {
                        AdminOnly = true,
                    });
                    commands.Add(new Command<GetRandomDog>(RND));
                    commands.Add(new Command<ShowRepo>(REPO));
                    commands.Add(new Command<GetDogOfTheDayCount>(COUNT));
                    commands.Add(new Command<ShowPermissions>(PERMISSION));
                }
                return commands.ToList();
            }
        }

        const string DOTD = "dotd";
        const string DOTDSUBMIT = "dotdsubmit";
        const string HELP = "dotdhelp";
        const string STATS = "dotdstats";
        const string MUTE = "dotdmute";
        const string UNMUTE = "dotdunmute";
        const string RND = "dotdrnd";
        const string REPO = "dotdrepo";
        const string COUNT = "dotdcount";
        const string PERMISSION = "dotdpermission";

        public const string COMMAND_TOKEN = "!";

        public static string Dotd { get { return Format(DOTD); } }

        public static string DotdSubmit(string url, string comment)
        {
            return string.Format("{0} {1} {2}", Format(DOTDSUBMIT), url, comment);
        }

        static string Format(string command)
        {
            return string.Format("{0}{1}", COMMAND_TOKEN, command);
        }
    }
}
