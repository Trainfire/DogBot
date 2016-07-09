using Core;

namespace Modules.CommandHandler
{
    class AddUser : ChatCommand
    {
        public override bool AdminOnly { get { return true; } }

        public override CommandResult Execute(CommandSource source)
        {
            var str = "";

            if (source.Parser.Args.Count > 0)
            {
                if (Bot.AddUser(source.Parser.Args[0]))
                {
                    str = "'{0}' is now a User";
                }
                else
                {
                    str = "Failed to add '{0}' as a User.";
                }
            }
            else
            {
                str = "Invalid number of arguments.";
            }

            return new CommandResult(str);
        }
    }
}
