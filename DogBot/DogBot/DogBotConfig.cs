using System;
using Core;

namespace BotDogBot
{
    public class DogBotConfig : FileStorage<DogBotConfigData>
    {
        public override string Filename
        {
            get
            {
                return GetType().Name.ToLower() + ".json";
            }
        }
    }

    public class DogBotConfigData
    {
        /// <summary>
        /// How many announcements to make before changing to the next one.
        /// </summary>
        public int AnnouncementAmount { get; set; }

        /// <summary>
        /// How often to make an announcement in seconds.
        /// </summary>
        public double AnnouncementInterval { get; set; }
    }
}
