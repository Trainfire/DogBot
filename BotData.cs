using System;
using SteamKit2;

namespace DogBot
{
    /// <summary>
    /// Information about the current state of the Bot.
    /// </summary>
    public class BotData
    {
        readonly History history;

        public DogData Dog { get; private set; }
        public HistoryStats HistoryStats { get { return history.Stats; } }

        public BotData()
        {
            Dog = new DogData();
            history = new History();
            history.Load();
        }

        public void SetDog(DogData dog)
        {
            Dog = dog;
            WriteToHistory(dog);
        }

        public void AddDog(DogData dog)
        {
            // Append the Dog to history but do not set it!
            WriteToHistory(dog);
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
