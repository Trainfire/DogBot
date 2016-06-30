using System;
using SteamKit2;
using System.Linq;
using System.Collections.Generic;

namespace BotDogBot
{
    /// <summary>
    /// Information about the current state of the Bot.
    /// </summary>
    public class BotData
    {
        public event EventHandler<DogData> DogSubmitted;

        readonly History history;

        DogData dog;
        DotdQueue queue;

        public HistoryStats HistoryStats { get { return history.Stats; } }
        public DogData CurrentDog { get { return queue.Peek(); } }
        public bool HasDog { get { return CurrentDog != null; } }
        public int QueueCount { get { return Queue.Count; } }
        public int TotalDogsShown { get { return HistoryStats.Dogs.Count; } }
        public int TotalDogsAdded { get { return HistoryStats.Dogs.Count + QueueCount; } }

        /// <summary>
        /// Returns a copy of the Queue.
        /// </summary>
        public List<DogData> Queue { get { return queue.Data.Queue.ToList(); } }

        public BotData()
        {
            history = new History();
            queue = new DotdQueue();
            history.Load();
        }

        public void EnqueueDog(DogData dog)
        {
            queue.Enqueue(dog);

            if (DogSubmitted != null)
                DogSubmitted(this, dog);
        }

        /// <summary>
        /// Gets the next Dog in the queue, if there is one, and removes it from the queue file.
        /// </summary>
        public void MoveToNextDog()
        {
            if (queue.Data.Queue.Count != 0)
            {
                dog = queue.Dequeue();
                WriteToHistory(dog);
            }
        }

        public List<DogData> GetUserContributions(SteamKit2.SteamID steamID)
        {
            var fromHistory = history.Stats.GetUserContributions(steamID);
            var fromQueue = queue.GetUserContributions(steamID);

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

        void WriteToHistory(DogData dog)
        {
            history.Write(new HistoryRecord()
            {
                Dog = dog,
            });
        }
    }
}
