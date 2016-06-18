using System;
using System.Timers;

namespace DogBot
{
    /// <summary>
    /// Makes an announcement once every hour over a 24 hour period.
    /// </summary>
    public class Announcer
    {
        /// <summary>
        /// Fired every time an announcement is made.
        /// </summary>
        public event EventHandler Announce;

        /// <summary>
        /// Fired once all announcements have been made.
        /// </summary>
        public event EventHandler AllAnnounced;

        readonly double timeBetweenAnnouncements;
        Timer timer;

        /// <summary>
        /// The number of announcements that have been made over the current day.
        /// </summary>
        public int AnnouncementsMade { get; private set; }

        /// <summary>
        /// How many announcements remain over a 24 hour period. One announcement is made every hour.
        /// </summary>
        public int AnnouncementsRemaining { get { return 24 - DateTime.Now.Hour; } }

        public Announcer(double timeBetweenAnnouncements)
        {
            this.timeBetweenAnnouncements = timeBetweenAnnouncements;
        }

        public void Start()
        {
            timer = new Timer(1000 * timeBetweenAnnouncements);
            timer.Elapsed += MakeAnnouncement;
            timer.Start();
        }

        void MakeAnnouncement(object sender, ElapsedEventArgs e)
        {
            if (Announce != null)
                Announce(this, null);

            AnnouncementsMade++;

            if (AnnouncementsMade == AnnouncementsRemaining)
            {
                if (AllAnnounced != null)
                    AllAnnounced(this, null);

                AnnouncementsMade = 0;
            }
        }
    }
}
