using System.Collections.Generic;
using Core;

namespace Modules.DogOfTheDay
{
    public class Queue : FileStorage<DotdQueueData>
    {
        public QueueController Controller { get; private set; }
             
        public Queue() : base()
        {
            Controller = new QueueController(Data.Queue);
            Controller.Changed += QueueLogic_Changed;
        }

        private void QueueLogic_Changed(object sender, List<DogData> data)
        {
            Data.Queue = data;
            Save();
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
