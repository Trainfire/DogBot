using System;
using System.Timers;
using System.Collections.Generic;
using Core;

namespace BotDogBot
{
    public class DogBot : Bot
    {
        Announcer announcer;
        DogBotConfig dogBotConfig;

        public BotData Data { get; private set; }

        #region Commands
        const string RND = "dotdrnd";
        const string REPO = "dotdrepo";
        const string COUNT = "dotdcount";
        const string DOTD = "dotd";
        const string DOTDSUBMIT = "dotdsubmit";
        #endregion

        protected override void Initialize()
        {
            base.Initialize();

            dogBotConfig = new DogBotConfig();

            Data = new BotData();

            // Create the announcer
            announcer = new Announcer(dogBotConfig.Data.AnnouncementInterval);
            announcer.Announce += OnAnnounce;
            announcer.AllAnnounced += OnAllAnnouncements;

            // Register commands
            CommandRegistry.AddCommand(new Command<GetDogOfTheDay>(DOTD));
            CommandRegistry.AddCommand(new Command<SubmitDogOfTheDay>(DOTDSUBMIT)
            {
                UsersOnly = true,
                HelpArgs = new List<string>()
                        {
                            "URL",
                            "Comment (Optional)"
                        }
            });
            CommandRegistry.AddCommand(new Command<GetRandomDog>(RND));
            CommandRegistry.AddCommand(new Command<GetDogOfTheDayCount>(COUNT));
        }

        protected override void OnDisconnected(object sender, EventArgs e)
        {
            base.OnDisconnected(sender, e);
            announcer.Stop();
        }

        protected override void OnJoinChat()
        {
            base.OnJoinChat();

            // Start the announcer timer upon joining chat.
            announcer.Start();

            Logger.Info("Announcements remaining for {0}: {1}", DateTime.Now.Day.ToString(), announcer.AnnouncementsRemaining);
        }

        void OnAnnounce(object sender, EventArgs e)
        {
            // Post DoTD
            if (Data.HasDog)
            {
                Logger.Info("Posting announcement...");
                HandleMessage(MessageContext.Chat, SID, DOTD);
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

        #region Commands
        public string Dotd { get { return CommandRegistry.Format(DOTD); } }

        public string DotdSubmit(string url, string comment)
        {
            return string.Format("{0} {1} {2}", CommandRegistry.Format(DOTDSUBMIT), url, comment);
        }
        #endregion

        public static class Strings
        {
            public const string SubmitDogOfTheDay = "Dog added to the queue!";
            public const string NoDog = " * whines * There is no Dog of the Day... If you have permission, use !dotdsubmit <URL> <Comment (Optional)> to submit a message.";
            public const string UrlInvalid = "*whines* That URL is invalid...";
            public const string Setter = "Dog of the Day was set by";
            public const string StatsTotalAdded = "Dogs added to this day: ";
            public const string StatsTotalShown = "Dogs shown: ";
            public const string StatsHighestContributer = "Highest contributor: ";
            public const string Repo = "https://github.com/Trainfire/DogBot";
            public const string TotalMessages = "Dog of the Day // Messages remaining: ";
        }
    }
}
