using System;
using SteamKit2;
using System.Linq;
using System.Collections.Generic;

namespace DogBot
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

        public Maps Maps { get; private set; }
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
            Maps = new Maps();
            queue = new DotdQueue();
            history.Load();
            Maps.Load();
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

        void WriteToHistory(DogData dog)
        {
            dog.TimeStamp = DateTime.UtcNow.ToBinary().ToString();
            history.Write(new HistoryRecord()
            {
                Dog = dog,
            });
        }
    }
}
