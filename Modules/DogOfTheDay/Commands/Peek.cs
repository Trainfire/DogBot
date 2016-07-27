using SteamKit2;
using System.Collections.Generic;
using Core;
using System.Threading.Tasks;
using System;

namespace Modules.DogOfTheDay
{
    class PeekAhead : DogOfTheDayCommand
    {
        public override bool AdminOnly
        {
            get
            {
                return true;
            }
        }

        public override CommandResult Execute(CommandSource source)
        {
            var peek = DogOfTheDay.Data.Queue.PeekAhead();
            if (peek != null)
            {
                return new CommandResult("Tomorrow's dog will be brought to you by {0}.", Bot.GetFriendName(peek.Setter));
            }
            else
            {
                return new CommandResult("There are no more dogs!");
            }
        }
    }
}
