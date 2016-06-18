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
            Data.Queue.Enqueue(dog);
        }
    }

    public class DotdQueueData
    {
        public Queue<DogData> Queue { get; set; }

        public DotdQueueData()
        {
            if (Queue == null)
                Queue = new Queue<DogData>();
        }
    }
}
