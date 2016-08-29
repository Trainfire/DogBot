﻿using Core;

namespace Modules.BotManager
{
    class BotManager : Module
    {
        protected override void OnInitialize()
        {
            base.OnInitialize();

            CommandListener.AddCommand("!~adduser", AddUser, true);
            CommandListener.AddCommand("!~removeuser", RemoveUser, true);
            CommandListener.AddCommand("!~setname", SetName, true);
        }

        string AddUser(CommandSource source)
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

        string RemoveUser(CommandSource source)
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

        string SetName(CommandSource source)
        {
            if (source.Parser.Args.Count > 0)
                Bot.Friends.SetPersonaName(source.Parser.Args[0]);

            return string.Empty;
        }
    }
}
