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
        /// How many announcements remain over a 24 hour period. One announcement is made every hour.
        /// </summary>
        public int AnnouncementsRemaining { get; private set; }

        public Announcer(double timeBetweenAnnouncements)
        {
            this.timeBetweenAnnouncements = timeBetweenAnnouncements;
        }

        public void Start()
        {
            AnnouncementsRemaining = 24 - DateTime.Now.Hour;

            Console.WriteLine("Remaining announcements for this day:"  + AnnouncementsRemaining);

            timer = new Timer(1000 * timeBetweenAnnouncements);
            timer.Elapsed += MakeAnnouncement;
            timer.Start();
        }

        void MakeAnnouncement(object sender, ElapsedEventArgs e)
        {
            if (Announce != null)
                Announce(this, null);

            AnnouncementsRemaining--;

            if (AnnouncementsRemaining == 0)
            {
                if (AllAnnounced != null)
                    AllAnnounced(this, null);

                AnnouncementsRemaining = 24;
            }
        }
    }
}
