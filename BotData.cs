using System;
using SteamKit2;
using System.Linq;

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

        public HistoryStats HistoryStats { get { return history.Stats; } }
        public DogData CurrentDog { get { return queue.Data.Queue.Count != 0 ? queue.Data.Queue.Peek() : null; } }
        public bool HasDog { get { return CurrentDog != null; } }
        public int QueueCount { get { return queue.Data.Queue.Count; } }

        public BotData()
        {
            history = new History();
            queue = new DotdQueue();
            history.Load();
        }

        public void EnqueueDog(DogData dog)
        {
            queue.Enqueue(dog);
            queue.Save();

            WriteToHistory(dog);

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
                dog = queue.Data.Queue.Dequeue();
                queue.Save();
            }
        }

        void WriteToHistory(DogData dog)
        {
            dog.TimeStamp = DateTime.UtcNow.ToBinary().ToString();
            history.Write(new HistoryRecord()
            {
                Dog = dog,
            });
            history.Save();
        }
    }
}
