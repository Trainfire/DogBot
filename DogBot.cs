using System;
using System.Timers;
using SteamKit2;

namespace DogBot
{
    public class DogBot : Bot
    {
        Announcer announcer;

        protected override void OnInitialize()
        {
            // Create the announcer
            announcer = new Announcer(Configuration.AnnouncementInterval);
            announcer.Announce += OnAnnounce;
            announcer.AllAnnounced += OnAllAnnouncements;
        }

        protected override void OnJoinChat()
        {
            // Start the announcer timer upon joining chat.
            announcer.Start();

            Logger.Info("Announcements remaining for {0}: {1}", DateTime.Now.Day.ToString(), announcer.AnnouncementsRemaining);
        }

        protected override void OnDisconnected(object sender, EventArgs e)
        {
            base.OnDisconnected(sender, e);
            announcer.Stop();
        }

        void OnAnnounce(object sender, EventArgs e)
        {
            // Post DoTD
            if (Data.HasDog)
            {
                Logger.Info("Posting announcement...");
                HandleMessage(MessageContext.Chat, SID, CommandRegistry.Dotd);
            }
        }

        void OnAllAnnouncements(object sender, EventArgs e)
        {
            Logger.Info("All announcements for current dog shown.");

            Data.MoveToNextDog();

            if (Data.HasDog)
            {
                Logger.Info("Moving to next dog in queue...");
            }
            else
            {
                Logger.Info("Nothing to announce.");
            }
        }
    }
}
