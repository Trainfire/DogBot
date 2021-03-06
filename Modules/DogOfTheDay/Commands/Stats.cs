using SteamKit2;
using System.Collections.Generic;
using Core;

namespace Modules.DogOfTheDay
{
    class Stats : DogOfTheDayCommand
    {
        public override string Execute(CommandSource source)
        {
            var stats = new List<string>();

            // Total records
            stats.Add(DogOfTheDay.Strings.StatsTotalAdded + DogOfTheDay.Data.TotalDogsAdded);

            // Dogs Shown
            stats.Add(DogOfTheDay.Strings.StatsTotalShown + DogOfTheDay.Data.TotalDogsShown);

            // Highest contributer
            var highestContributor = DogOfTheDay.Data.HighestContributor;
            if (highestContributor != null)
            {
                var name = Bot.Names.GetFriendName(highestContributor);
                stats.Add(string.Format("{0}{1} ({2})", DogOfTheDay.Strings.StatsHighestContributer, name, DogOfTheDay.Data.GetUserContributions(highestContributor).Count));
            }

            // Include caller's contributions if they have any.
            var callerContributions = DogOfTheDay.Data.GetUserContributions(source.Caller);
            if (callerContributions.Count != 0)
            {
                var callerName = Bot.Names.GetFriendName(source.Caller);
                stats.Add(string.Format("{0}'s total submissions: {1}", callerName, callerContributions.Count));
            }

            return string.Join(" // ", stats.ToArray());
        }
    }
}
