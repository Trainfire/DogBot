using Core;

namespace Modules.BotManager
{
    class BotManager : Module
    {
        protected override void OnInitialize()
        {
            base.OnInitialize();

            AddCommand<AddUser>("~adduser");
            AddCommand<RemoveUser>("~removeuser");
            AddCommand<SetName>("~setname");
        }
    }

    class AddUser : ChatCommand
    {
        public override bool AdminOnly { get { return true; } }

        public override string Execute(CommandSource source)
        {
            var str = "";

            if (source.Parser.Args.Count > 0)
            {
                var arg = source.Parser.Args[0];

                if (Bot.AddUser(source.Parser.Args[0]))
                {
                    str = string.Format("'{0}' is now a User", Bot.GetFriendName(arg));
                }
                else
                {
                    str = string.Format("Failed to add '{0}' as a User.", Bot.GetFriendName(arg));
                }
            }
            else
            {
                str = "Invalid number of arguments.";
            }

            return str;
        }
    }

    class RemoveUser : ChatCommand
    {
        public override bool AdminOnly { get { return true; } }

        public override string Execute(CommandSource source)
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

            return str;
        }
    }

    class SetName : ChatCommand
    {
        public override bool AdminOnly { get { return true; } }

        public override string Execute(CommandSource source)
        {
            if (source.Parser.Args.Count > 0)
                Bot.Friends.SetPersonaName(source.Parser.Args[0]);

            return string.Empty;
        }
    }
}
