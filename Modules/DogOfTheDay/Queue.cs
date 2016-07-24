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

        List<DogData> data;

        public int Count { get { return data.Count; } }
             
        public Queue() : base()
        {
            data = Data.Queue;
        }

        public void Enqueue(DogData dog)
        {
            data.Add(dog);
            Save();

            if (DataChanged != null)
                DataChanged(this, this);
        }

        public DogData Peek()
        {
            return data.Count != 0 ? data[0] : null;
        }

        public DogData Dequeue()
        {
            if (data.Count != 0)
            {
                // Reorder submissions so the queue prioritises unique authors
                // This is to prevent a series of submissions from the same person blocking the queue
                // for potentially several days!
                var distinct = data
                    .GroupBy(x => x.Setter)
                    .Select(x => x.First())
                    .OrderBy(x => x.TimeStamp)
                    .ToList();

                // Find and remove duplicate values
                var duplicates = data.Intersect(distinct).ToList();
                duplicates.ForEach(x => distinct.Remove(x));

                // Update queue
                data = distinct.Concat(data).ToList();

                // Get the first dog in the queue.
                var dog = data[0];

                // Remove the dog from the queue.
                data.Remove(dog);

                Save();

                if (DataChanged != null)
                    DataChanged(this, this);

                return dog;
            }
            return null;
        }

        public List<DogData> GetUserContributions(SteamID steamID)
        {
            return data.Where(x => x.Setter == steamID).ToList();
        }

        public List<SteamID> GetUsers()
        {
            return data
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
