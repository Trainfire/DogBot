using Core;

namespace Core
{
    class RemoveUser : ChatCommand
    {
        public override bool AdminOnly { get { return true; } }

        public override CommandResult Execute(CommandSource source)
        {
            var str = "";

            if (source.Parser.Args.Count > 0)
            {
                var arg = source.Parser.Args[0];

                if (Bot.RemoveUser(source.Parser.Args[0]))
                {
                    str = string.Format("'{0}' is no longer a User.", Bot.GetFriendName(arg));
                }
                else
                {
                    str = string.Format("Failed to remove '{0}' from users.", Bot.GetFriendName(arg));
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
