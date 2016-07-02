using System;
using System.Collections.Generic;
using System.Linq;
using SteamKit2;
using Core;

namespace Modules.DogOfTheDay
{
    public class Queue : FileStorage<DotdQueueData>
    {        
        public void Enqueue(DogData dog)
        {
            Data.Queue.Add(dog);
            Save();
        }

        public DogData Peek()
        {
            return Data.Queue.Count != 0 ? Data.Queue[0] : null;
        }

        public DogData Dequeue()
        {
            if (Data.Queue.Count != 0)
            {
                // Reorder submissions so the queue prioritises unique authors
                // This is to prevent a series of submissions from the same person blocking the queue
                // for potentially several days!
                var distinct = Data.Queue
                    .GroupBy(x => x.Setter)
                    .Select(x => x.First())
                    .OrderBy(x => x.TimeStamp)
                    .ToList();

                // Remove non-distinct entries
                distinct.ForEach(x => Data.Queue.Remove(x));

                // Update queue
                Data.Queue = distinct.Concat(Data.Queue).ToList();

                var dog = Data.Queue[0];
                Data.Queue.Remove(dog);

                Save();

                return dog;
            }
            return null;
        }

        public List<DogData> GetUserContributions(SteamID steamID)
        {
            return Data.Queue.Where(x => x.Setter == steamID).ToList();
        }
    }

    public class DotdQueueData
    {
        public List<DogData> Queue { get; set; }

        public DotdQueueData()
        {
            if (Queue == null)
                Queue = new List<DogData>();
        }
    }
}