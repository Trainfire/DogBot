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

            Save();

            if (DataChanged != null)
                DataChanged(this, this);
        }

        public void Sort()
        {
            // Get one submission per author.
            var distinct = Data.Queue
                .GroupBy(x => x.Setter)
                .Select(x => x.First())
                .OrderBy(x => x.TimeStamp)
                .ToList();

            Data.Active.Clear();
            Data.Active.AddRange(distinct);

            // Find and remove duplicate values from the backlog.
            distinct.ForEach(x => Data.Queue.Remove(x));
        }

        public List<DogData> Active
        {
            get { return Data.Active; }
        }

        public DogData PeekAhead()
        {
            return Data.Queue.Count > 1 ? Data.Queue[1] : null;
        }

        public DogData Peek()
        {
            if (Data.Active.Count != 0)
            {
                return Data.Active[0];
            }
            else if (Data.Queue.Count != 0)
            {
                return Data.Queue[0];
            }
            return null;
        }

        public DogData Dequeue(bool save = true)
        {
            var dog = GetNextActive();

            // Active queue is empty. Sort then try again.
            if (dog == null && Data.Queue.Count != 0)
            {
                Sort();
                dog = GetNextActive();
            }

            if (save)
                Save();

            if (DataChanged != null)
                DataChanged(this, this);

            return dog;
        }

        public DogData GetNextActive()
        {
            if (Data.Active.Count != 0)
            {
                var next = Data.Active[0];
                Data.Active.RemoveAt(0);
                return next;
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
        public List<DogData> Active { get; set; }
        public List<DogData> Queue { get; set; }

        public QueueData()
        {
            if (Active == null)
                Active = new List<DogData>();

            if (Queue == null)
                Queue = new List<DogData>();
        }
    }
}
