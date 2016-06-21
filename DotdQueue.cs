using System;
using System.Collections.Generic;
using System.Linq;

namespace DogBot
{
    public class DotdQueue : FileLogger<DotdQueueData>
    {
        public override string Filename
        {
            get
            {
                return "queue.json";
            }
        }
        
        public void Enqueue(DogData dog)
        {
            Data.Queue.Add(dog);
        }

        public DogData Peek()
        {
            return Data.Queue.Count != 0 ? Data.Queue[0] : null;
        }

        public DogData Dequeue()
        {
            if (Data.Queue.Count != 0)
            {
                // Reorder submissions so the top of the queue contains unique submission authors
                // This is to prevent a series of submissions from the same person blocking the queue
                // for potentially several days!
                var distinct = Data.Queue
                    .GroupBy(x => x.Setter)
                    .Select(x => x.First())
                    .ToList();

                distinct.ForEach(x => Data.Queue.ToList().Remove(x));
                distinct.Concat(Data.Queue);

                var dog = Data.Queue[0];
                Data.Queue.Remove(dog);
                Data.Queue = distinct;
                return dog;
            }
            return null;
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
