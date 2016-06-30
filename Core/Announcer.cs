using System;
using System.Timers;

namespace Core
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

            timer = new Timer(GetMillisecondsToHour());
            timer.Elapsed += MakeAnnouncement;
            timer.Start();
        }

        /// <summary>
        /// Stops the announcer from ticking.
        /// </summary>
        public void Stop()
        {
            if (timer != null)
            {
                timer.Elapsed -= MakeAnnouncement;
                timer.Stop();
            }
        }

        void MakeAnnouncement(object sender, ElapsedEventArgs e)
        {
            if (Announce != null)
                Announce(this, null);

            AnnouncementsRemaining--;

            timer.Interval = GetMillisecondsToHour();

            if (AnnouncementsRemaining == 0)
            {
                if (AllAnnounced != null)
                    AllAnnounced(this, null);

                AnnouncementsRemaining = 24;
            }
        }

        int GetMillisecondsToHour()
        {
            int interval;

            int minutesRemaining = 59 - DateTime.Now.Minute;
            int secondsRemaining = 59 - DateTime.Now.Second;

            interval = ((minutesRemaining * 60) + secondsRemaining) * 1000;

            if (interval == 0)
                interval = 60 * 60 * 1000;

            return interval;
        }
    }
}
