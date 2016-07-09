using Core;

namespace Modules.CommandHandler
{
    class RemoveUser : ChatCommand
    {
        public override bool AdminOnly { get { return true; } }

        public override CommandResult Execute(CommandSource source)
        {
            var str = "";

            if (source.Parser.Args.Count > 0)
            {
                if (Bot.RemoveUser(source.Parser.Args[0]))
                {
                    str = "'{0}' is no longer a User.";
                }
                else
                {
                    str = "Failed to remove '{0}' from users.";
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
