using System;
using System.Timers;
using System.Collections.Generic;
using Core;
using SteamKit2;

namespace Modules.DogOfTheDay
{
    public class DogOfTheDay : Module
    {
        Announcer announcer;
        Config dogBotConfig;

        public BotData Data { get; private set; }

        #region Commands
        const string RND = "dotdrnd";
        const string REPO = "dotdrepo";
        const string COUNT = "dotdcount";
        const string DOTD = "dotd";
        const string DOTDSUBMIT = "dotdsubmit";
        #endregion

        protected override void OnInitialize()
        {
            dogBotConfig = new Config();

            Data = new BotData();

            // Create the announcer
            announcer = new Announcer(dogBotConfig.Data.AnnouncementInterval);
            announcer.Announce += OnAnnounce;
            announcer.AllAnnounced += OnAllAnnouncements;
        }

        public override List<Command> Commands
        {
            get
            {
                return new List<Command>()
                {
                    new Command<GetDogOfTheDay>(DOTD),
                    new Command<SubmitDogOfTheDay>(DOTDSUBMIT)
                    {
                        UsersOnly = true,
                        HelpArgs = new List<string>()
                                {
                                    "URL",
                                    "Comment (Optional)"
                                }
                    },
                    new Command<GetRandomDog>(RND),
                    new Command<GetDogOfTheDayCount>(COUNT)
                };
            }
        }

        public override void OnDisconnect()
        {
            announcer.Stop();
        }

        public override void OnJoinChat(SteamID chatroomID)
        {
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
                Bot.ProcessMessageInternally(Bot.MessageContext.Chat, DOTD);
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
