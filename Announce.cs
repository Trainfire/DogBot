using System;
using System.Timers;

namespace DogBot
{
    public class Announcer
    {
        public event EventHandler Announce;
        public event EventHandler AllAnnounced;

        readonly int totalAnnouncements;
        readonly float timeBetweenAnnouncements;
        Timer timer;

        int announcementsMade = 0;

        public Announcer(double timeBetweenAnnouncements, int totalAnnouncements)
        {
            this.totalAnnouncements = totalAnnouncements;
        }

        public void Start()
        {
            timer = new Timer(1000 * timeBetweenAnnouncements);
            timer.Elapsed += MakeAnnouncement;
        }

        void MakeAnnouncement(object sender, ElapsedEventArgs e)
        {
            if (Announce != null)
                Announce(this, null);

            announcementsMade++;

            if (announcementsMade == totalAnnouncements)
            {
                if (AllAnnounced != null)
                    AllAnnounced(this, null);

                announcementsMade = 0;
            }
        }
    }
}
