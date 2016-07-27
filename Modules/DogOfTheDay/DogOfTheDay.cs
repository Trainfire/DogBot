using System;
using Core;

namespace Modules.DogOfTheDay
{
    public class DogOfTheDay : Module, ICommandHandler
    {
        Announcer announcer;
        Config Config;
        ChatCommandProcessor commandProcessor;
        CommandListener commandListener;

        public Data Data { get; private set; }
        public bool SyncEnabled { get { return Config.Data.SyncEnabled; } }
        public string SpreadsheetID { get { return Config.Data.SpreadsheetID; } }

        public bool Muted
        {
            set
            {
                commandProcessor.Muted = value;
                if (commandProcessor.Muted)
                {
                    Bot.SayToChat(Bot.CurrentChatRoomID, Strings.Muted);
                }
                else
                {
                    Bot.SayToChat(Bot.CurrentChatRoomID, Strings.Unmuted);
                }
            }
        }

        #region Commands
        const string RND = "dotdrnd";
        const string REPO = "dotdrepo";
        const string COUNT = "dotdcount";
        const string DOTD = "dotd";
        const string DOTDSUBMIT = "dotdsubmit";
        const string MOVENEXT = "dotdmovenext";
        const string STATS = "dotdstats";
        const string MUTE = "dotdmute";
        const string UNMUTE = "dotdunmute";
        const string ADDUSER = "dotdadduser";
        const string SYNC = "dotdsync";
        const string QUEUE = "dotdqueue";
        #endregion

        protected override void OnInitialize()
        {
            Config = new Config();
            commandProcessor = new ChatCommandProcessor(Bot);
            commandProcessor.NoPermissionMessage = Strings.NoPermission;
            Data = new Data(this);

            // Create the announcer
            announcer = new Announcer();
            announcer.Announce += OnAnnounce;
            announcer.AllAnnounced += OnAllAnnouncements;

            // Make an announcement once every hour.
            for (int i = 0; i < 24; i++)
            {
                announcer.AddTime(i, 59, 59);
            }

            announcer.Start();

            Logger.Info("Announcements remaining for {0}: {1}", DateTime.Now.DayOfWeek.ToString(), announcer.AnnouncementsRemaining);

            commandListener = new CommandListener(Bot);
            commandListener.AddCommand<GetDogOfTheDay>(DOTD, this);
            commandListener.AddCommand<GetDogOfTheDayCount>(COUNT, this);
            commandListener.AddCommand<GetRandomDog>(RND, this);
            commandListener.AddCommand<MoveNext>(MOVENEXT, this);
            commandListener.AddCommand<Stats>(STATS, this);
            commandListener.AddCommand<SubmitDogOfTheDay>(DOTDSUBMIT, this);
            commandListener.AddCommand<Mute>(MUTE, this);
            commandListener.AddCommand<Unmute>(UNMUTE, this);
            commandListener.AddCommand<Sync>(SYNC, this);
            commandListener.AddCommand<QueueInfo>(QUEUE, this);
        }

        void ICommandHandler.OnCommandTriggered(CommandEvent commandEvent)
        {
            commandProcessor.ProcessCommand(commandEvent);
        }

        void OnAnnounce(object sender, EventArgs e)
        {
            // Post DoTD
            if (Data.HasDog)
            {
                Logger.Info("Posting announcement...");
                commandListener.FireCommand(DOTD);
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

        public string GetDogOfTheDay()
        {
            return string.Format("{0}'s {1} // {2}", DateTime.Now.DayOfWeek.ToString(), Strings.DogOfTheDay, Data.CurrentDog.URL);
        }

        public static class Strings
        {
            public const string DogOfTheDay = "Dog of the Day";
            public const string SubmitDogOfTheDay = "Dog added to the queue!";
            public const string SubmitURLExists = "* whines * Cannot accept that URL as it's already in the queue...";
            public const string NoDog = " * whines * There is no Dog of the Day... If you have permission, use !dotdsubmit <URL> <Comment (Optional)> to submit a message.";
            public const string UrlInvalid = "*whines* That URL is invalid...";
            public const string Setter = "Dog of the Day was set by";
            public const string StatsTotalAdded = "Dogs added to this day: ";
            public const string StatsTotalShown = "Dogs shown: ";
            public const string StatsHighestContributer = "Highest contributor: ";
            public const string Repo = "https://github.com/Trainfire/DogBot";
            public const string TotalMessages = "Dog of the Day // Messages remaining: ";
            public const string Muted = "*muted*";
            public const string Unmuted = "*bark!*";
            public const string NoPermission = "*bark* You don't have permission to do that!";
        }
    }
}
