using SteamKit2;
using System.Collections.Generic;
using System.Linq;

namespace Modules.ThingOfTheDay
{
    class StatsHelper
    {
        public int TotalShown { get; private set; }
        public int TotalSubmissions { get; private set; }
        public List<SteamID> HighestContributers { get; private set; }

        List<Content> _history;
        List<Content> _queue;

        public StatsHelper(List<Content> queue, List<Content> history)
        {
            _queue = queue != null ? queue : new List<Content>();
            _history = history != null ? history : new List<Content>();

            TotalShown = _history.Count;
            TotalSubmissions = _history.Count + _queue.Count;
        }

        public List<Content> GetUserContributions(SteamID steamID)
        {
            return _history.Where(x => x.Setter == steamID).ToList();
        }

        public SteamID HighestContributor
        {
            get
            {
                var list = _history.Concat(_queue).ToList();

                var highest = list
                    .GroupBy(x => x.Setter)
                    .OrderByDescending(x => x.Count())
                    .Select(x => x.Key)
                    .FirstOrDefault();

                return highest;
            }
        }
    }
}
