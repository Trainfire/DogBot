using System;
using System.Collections.Generic;
using System.Linq;
using SteamKit2;

namespace Modules.DogOfTheDay
{
    /// <summary>
    /// Manages the queue order and fires the Changed callback whenever the queue is modified.
    /// </summary>
    public class QueueController
    {
        public event EventHandler<List<DogData>> Changed;

        List<DogData> data;

        public QueueController(List<DogData> data)
        {
            this.data = data;
        }

        public void Enqueue(DogData dog)
        {
            data.Add(dog);
            OnChange();
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

                OnChange();

                return dog;
            }
            return null;
        }

        public List<DogData> GetUserContributions(SteamID steamID)
        {
            return data.Where(x => x.Setter == steamID).ToList();
        }

        void OnChange()
        {
            if (Changed != null)
                Changed(this, data);
        }
    }
}
