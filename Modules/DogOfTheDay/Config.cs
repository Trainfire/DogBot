using System;
using Core;

namespace Modules.DogOfTheDay
{
    public class Config : FileStorage<ConfigData> { }

    public class ConfigData
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
