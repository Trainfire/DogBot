using SteamKit2;
using System.Collections.Generic;
using Core;

namespace BotDogBot
{
    class Stats : DogBotCommandAction
    {
        public override CommandResult Execute(Bot bot, SteamID caller, MessageParser parser)
        {
            var stats = new List<string>();

            // Total records
            stats.Add(DogBot.Strings.StatsTotalAdded + DogBot.Data.TotalDogsAdded);

            // Dogs Shown
            stats.Add(DogBot.Strings.StatsTotalShown + DogBot.Data.TotalDogsShown);

            // Highest contributer
            var highestContributor = DogBot.Data.HighestContributor;
            if (highestContributor != null)
            {
                var name = bot.GetFriendName(highestContributor);
                stats.Add(string.Format("{0}{1} ({2})", DogBot.Strings.StatsHighestContributer, name, DogBot.Data.GetUserContributions(highestContributor).Count));
            }

            // Include caller's contributions if they have any.
            var callerContributions = DogBot.Data.GetUserContributions(caller);
            if (callerContributions.Count != 0)
            {
                var callerName = bot.GetFriendName(caller);
                stats.Add(string.Format("{0}'s total submissions: {1}", callerName, callerContributions.Count));
            }

            return new CommandResult(string.Join(" // ", stats.ToArray()));
        }
    }
}
