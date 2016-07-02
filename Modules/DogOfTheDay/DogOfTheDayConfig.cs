using System;
using Core;

namespace Modules.DogOfTheDay
{
    public class DogOfTheDayConfig : FileStorage<DogOfTheDayConfigData>
    {
        public override string Filename
        {
            get
            {
                return GetType().Name.ToLower() + ".json";
            }
        }
    }

    public class DogOfTheDayConfigData
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
