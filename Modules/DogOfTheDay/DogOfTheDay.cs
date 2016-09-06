using System;
using Core;

namespace Modules.DogOfTheDay
{
    public enum AnnouncementMode
    {
        Hourly,
        Daily,
    }

    public class DogOfTheDay : Module
    {
        Announcer announcer;
        Config Config;

        public Strings Strings { get; private set; }
        public Data Data { get; private set; }
        public bool SyncEnabled { get { return Config.Data.SyncEnabled; } }
        public string SpreadsheetID { get { return Config.Data.SpreadsheetID; } }
        public AnnouncementMode AnnouncementMode { get; private set; }

        public bool Muted
        {
            set
            {
                // TODO: Fix. Probably want to move this into the Bot or CommandListener.
                //CommandParser.Muted = value;
                //if (CommandParser.Muted)
                //{
                //    Bot.SayToChat(Bot.CurrentChatRoomID, Strings.Muted);
                //}
                //else
                //{
                //    Bot.SayToChat(Bot.CurrentChatRoomID, Strings.Unmuted);
                //}
            }
        }

        #region Commands
        const string RND = "!dotdrnd";
        const string REPO = "!dotdrepo";
        const string COUNT = "!dotdcount";
        const string DOTD = "!dotd";
        const string DOTDSUBMIT = "!dotdsubmit";
        const string MOVENEXT = "!dotdmovenext";
        const string STATS = "!dotdstats";
        const string MUTE = "!dotdmute";
        const string UNMUTE = "!dotdunmute";
        const string ADDUSER = "!dotdadduser";
        const string SYNC = "!dotdsync";
        const string QUEUE = "!dotdqueue";
        const string PEEK = "!dotdpeek";
        const string SORT = "!dotdsort";
        #endregion

        protected override void OnInitialize()
        {
            Config = new Config();
            Data = new Data(this);
            Strings = new Strings(this);

            // Set announcement mode
            AnnouncementMode = Config.Data.AnnouncementMode;

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

            CommandListener.AddCommand<GetDogOfTheDay>(DOTD);
            CommandListener.AddCommand<GetDogOfTheDayCount>(COUNT);
            CommandListener.AddCommand<GetRandomDog>(RND);
            CommandListener.AddCommand<MoveNext>(MOVENEXT);
            CommandListener.AddCommand<Stats>(STATS);
            CommandListener.AddCommand<SubmitDogOfTheDay>(DOTDSUBMIT);
            CommandListener.AddCommand<Mute>(MUTE);
            CommandListener.AddCommand<Unmute>(UNMUTE);
            CommandListener.AddCommand<Sync>(SYNC);
            CommandListener.AddCommand<QueueInfo>(QUEUE);
            CommandListener.AddCommand<PeekAhead>(PEEK);
            CommandListener.AddCommand<Sort>(SORT);
            CommandListener.AddCommand("!dogtoggle", ToggleAnnouncementMode, true);
        }

        void OnAnnounce(object sender, EventArgs e)
        {
            if (!Bot.Connection.Connected)
            {
                Logger.Warning("Bot is not connected. Announcement cancelled...");
                return;
            }

            if (Config.Data.AnnouncementMode == AnnouncementMode.Hourly)
            {
                Logger.Info("Moving to next dog in queue...");
                Data.MoveToNextDog();
            }

            // Post DoTD
            if (Data.HasDog)
            {
                Logger.Info("Posting announcement...");
                CommandListener.FireCommand(DOTD);
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
        string ToggleAnnouncementMode(CommandSource source)
        {
            AnnouncementMode = AnnouncementMode == AnnouncementMode.Daily ? AnnouncementMode.Hourly : AnnouncementMode.Daily;
            return "Dogs will now change " + AnnouncementMode.ToString().ToLower() + ".";
        }
        #endregion
    }

    public class Strings
    {
        DogOfTheDay dotd;

        public Strings(DogOfTheDay dotd)
        {
            this.dotd = dotd;
        }

        public readonly string DogOfTheDay = "Dog of the Day";
        public readonly string DogOfTheHour = "Dog of the Hour";

        public string Setter
        {
            get
            {
                return dotd.AnnouncementMode == AnnouncementMode.Daily ? "Dog of the Day was set by" : "Dog of the Hour was set by";
            }
        }

        public string TotalMessages
        {
            get
            {
                return dotd.AnnouncementMode == AnnouncementMode.Daily ? "Dog of the Day // Messages remaining: " : "Dog of the Hour // Messages remaining: ";
            }
        }

        public readonly string SubmitDogOfTheDay = "Dog added to the queue!";
        public readonly string SubmitURLExists = "* whines * Cannot accept that URL as it's already in the queue...";
        public readonly string NoDog = " * whines * There is no Dog of the Day... If you have permission, use !dotdsubmit <URL> <Comment (Optional)> to submit a message.";
        public readonly string StatsTotalAdded = "Dogs added to this day: ";
        public readonly string StatsTotalShown = "Dogs shown: ";
        public readonly string StatsHighestContributer = "Highest contributor: ";
        public readonly string Repo = "https://github.com/Trainfire/DogBot";
        public readonly string Muted = "*muted*";
        public readonly string Unmuted = "*bark!*";
        public readonly string NoPermission = "*bark* You don't have permission to do that!";
    }
}
