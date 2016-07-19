using System;
using SteamKit2;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core;
using Extensions.GoogleSpreadsheets;

namespace Modules.DogOfTheDay
{
    /// <summary>
    /// Information about the current state of the Bot.
    /// </summary>
    public class Data
    {
        public event EventHandler<DogData> DogSubmitted;

        readonly History history;
        readonly DogOfTheDay dotd;

        DogData dog;
        Queue queue;

        public HistoryStats HistoryStats { get { return history.Stats; } }
        public DogData CurrentDog { get { return queue.Controller.Peek(); } }
        public bool HasDog { get { return CurrentDog != null; } }
        public int QueueCount { get { return Queue.Count; } }
        public int TotalDogsShown { get { return HistoryStats.Dogs.Count; } }
        public int TotalDogsAdded { get { return HistoryStats.Dogs.Count + QueueCount; } }

        /// <summary>
        /// Returns a copy of the Queue.
        /// </summary>
        public List<DogData> Queue { get { return queue.Data.Queue.ToList(); } }

        public Data(DogOfTheDay dotd)
        {
            this.dotd = dotd;

            history = new History();
            queue = new Queue();
        }

        public async void EnqueueDog(DogData dog)
        {
            queue.Controller.Enqueue(dog);

            if (DogSubmitted != null)
                DogSubmitted(this, dog);

            await Sync();
        }

        /// <summary>
        /// Gets the next Dog in the queue, if there is one, and removes it from the queue file.
        /// </summary>
        public async void MoveToNextDog()
        {
            if (queue.Data.Queue.Count != 0)
            {
                dog = queue.Controller.Dequeue();
                WriteToHistory(dog);
                await Sync();
            }
        }

        public List<DogData> GetUserContributions(SteamKit2.SteamID steamID)
        {
            var fromHistory = history.Stats.GetUserContributions(steamID);
            var fromQueue = queue.Controller.GetUserContributions(steamID);

            return fromHistory.Concat(fromQueue).ToList();
        }

        public SteamID HighestContributor
        {
            get
            {
                var fromHistory = history.Stats.Dogs;
                var fromQueue = queue.Data.Queue;

                var list = fromHistory.Concat(queue.Data.Queue).ToList();

                var highest = list
                    .GroupBy(x => x.Setter)
                    .OrderByDescending(x => x.Count())
                    .Select(x => x.Key)
                    .FirstOrDefault();

                return highest;
            }
        }

        /// <summary>
        /// Syncs the data to Google.
        /// </summary>
        /// <returns></returns>
        public async Task Sync()
        {
            if (string.IsNullOrEmpty(dotd.SpreadsheetID) || !dotd.SyncEnabled)
                return;

            var spreadSheet = new Spreadsheet(dotd.SpreadsheetID);
            await spreadSheet.Get();

            var dogSheet = await spreadSheet.GetOrAddSheet("Dogs");

            Queue.ForEach(x =>
            {
                dogSheet.AddRow(new List<object>()
                {
                    dotd.Bot.GetFriendName(x.Setter),
                    x.Setter.ToString(),
                    TimestampToDate(x.TimeStamp),
                    x.URL,
                });
            });

            var historySheet = await spreadSheet.GetOrAddSheet("History");

            history.Data.History.ForEach(x =>
            {
                historySheet.AddRow(new List<object>()
                {
                    dotd.Bot.GetFriendName(x.Dog.Setter),
                    x.Dog.Setter.ToString(),
                    TimestampToDate(x.Dog.TimeStamp),
                    x.Dog.URL,
                });
            });

            await dogSheet.PushAsync();
            await historySheet.PushAsync();
        }

        void WriteToHistory(DogData dog)
        {
            history.Write(new HistoryRecord()
            {
                Dog = dog,
            });
        }

        string TimestampToDate(string timestamp)
        {
            var dt = DateTime.FromBinary(long.Parse(timestamp));
            return dt.ToShortDateString() + " " + dt.ToShortTimeString();
        }
    }
}
