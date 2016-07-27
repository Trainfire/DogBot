using System.Collections.Generic;
using Core;
using System.Linq;
using SteamKit2;
using System;

namespace Modules.DogOfTheDay
{
    public class Queue : FileStorage<QueueData>
    {
        public event EventHandler<Queue> DataChanged;

        public int Count { get { return Data.Queue.Count; } }

        public bool HasURL(string url)
        {
            return Data.Queue.Any(x => x.URL == url);
        }

        public void Enqueue(DogData dog)
        {
            Data.Queue.Add(dog);

            Sort();

            Save();

            if (DataChanged != null)
                DataChanged(this, this);
        }

        public void Sort()
        {
            // Reorder submissions so the queue prioritises unique authors
            // This is to prevent a series of submissions from the same person blocking the queue
            // for potentially several days!
            var distinct = Data.Queue
                .GroupBy(x => x.Setter)
                .Select(x => x.First())
                .OrderBy(x => x.TimeStamp)
                .ToList();

            // Find and remove duplicate values
            var duplicates = Data.Queue.Intersect(distinct).ToList();
            duplicates.ForEach(x => Data.Queue.Remove(x));

            // Update queue
            distinct.AddRange(Data.Queue);

            Data.Queue = distinct;
        }

        public DogData Peek()
        {
            return Data.Queue.Count != 0 ? Data.Queue[0] : null;
        }

        public DogData Dequeue(bool save = true)
        {
            if (Data.Queue.Count != 0)
            {
                // Remove the current dog from the queue.
                Data.Queue.Remove(Data.Queue[0]);

                if (save)
                    Save();

                if (DataChanged != null)
                    DataChanged(this, this);

                return Data.Queue[0];
            }
            return null;
        }

        public List<DogData> GetUserContributions(SteamID steamID)
        {
            return Data.Queue.Where(x => x.Setter == steamID).ToList();
        }

        public List<SteamID> GetUsers()
        {
            return Data.Queue
                .Select(x => x.Setter)
                .Distinct()
                .ToList();
        }
    }

    public class QueueData
    {
        public List<DogData> Queue { get; set; }

        public QueueData()
        {
            if (Queue == null)
                Queue = new List<DogData>();
        }
    }
}
