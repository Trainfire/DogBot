using System;
using System.Timers;
using System.Collections.Generic;
using System.Linq;

namespace Core
{
    /// <summary>
    /// Makes an daily announcements at specified intervals.
    /// </summary>
    public class Announcer
    {
        /// <summary>
        /// Fired every time an announcement is made.
        /// </summary>
        public event EventHandler Announce;

        /// <summary>
        /// Fired once all announcements have been made for the current day.
        /// </summary>
        public event EventHandler AllAnnounced;

        class Announcement
        {
            public event EventHandler Triggered;

            public DateTime Time { get; private set; }
            public bool Announced { get; private set; }

            public Announcement(int hour, int minute, int second)
            {
                Time = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hour, minute, second);

                // Flag as announced if date is in the past.
                if (Time < DateTime.Now)
                    Announced = true;
            }

            public void Announce()
            {
                Announced = true;
                Time = Time.AddDays(1);

                if (Triggered != null)
                    Triggered(this, null);
            }

            public void Reset()
            {
                Announced = false;
            }
        }

        Announcement nextAnnouncement;
        Timer timer;
        List<Announcement> announcements;

        /// <summary>
        /// How many announcements remain over a 24 hour period.
        /// </summary>
        public int AnnouncementsRemaining { get { return announcements.Where(x => !x.Announced).Count(); } }

        public Announcer()
        {
            announcements = new List<Announcement>();
            timer = new Timer(1000);
            timer.Elapsed += Tick;
        }

        public void AddTime(int hour, int minute, int second)
        {
            announcements.Add(new Announcement(hour, minute, second));
        }

        public void Start()
        {
            GetNextTime();
            timer.Start();
        }

        public void Stop()
        {
            timer.Stop();
        }

        void Tick(object sender, ElapsedEventArgs e)
        {
            if (DateTime.Now > nextAnnouncement.Time)
                MakeAnnouncement(this, e);
        }

        void MakeAnnouncement(object sender, ElapsedEventArgs e)
        {
            if (Announce != null)
                Announce(this, null);

            nextAnnouncement.Announce();

            if (announcements.TrueForAll(x => x.Announced))
            {
                if (AllAnnounced != null)
                    AllAnnounced(this, null);

                announcements.ForEach(x => x.Reset());
            }

            GetNextTime();
        }

        void GetNextTime()
        {
            foreach (var announcement in announcements)
            {
                if (announcement.Time > DateTime.Now)
                {
                    nextAnnouncement = announcement;
                    return;
                }
            }
        }
    }
}
