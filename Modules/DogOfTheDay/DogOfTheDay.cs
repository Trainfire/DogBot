using System;
using System.Linq;
using Core;
using SteamKit2;
using System.Collections.Generic;

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

        const string DOTD = "!dotd";

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
            CommandListener.AddCommand<GetDogOfTheDayCount>("!dotdcount");
            CommandListener.AddCommand<Stats>("!dotdstats");
            CommandListener.AddCommand<SubmitDogOfTheDay>("!dotdsubmit");
            CommandListener.AddCommand("!dogrnd", GetRandomDog);

            CommandListener.AddCommand("!dogpost", Post, true);
            CommandListener.AddCommand("!dogmovenext", MoveNext, true);
            CommandListener.AddCommand("!dogmute", Mute, true);
            CommandListener.AddCommand("!dogunmute", Unmute, true);
            CommandListener.AddCommand("!dogsort", Sort, true);
            CommandListener.AddCommand("!dogqueue", QueueInfo, true);
            CommandListener.AddCommand("!dogpeek", Peek, true);
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

        string Mute(CommandSource source)
        {
            Muted = true;
            return string.Empty;
        }

        string Unmute(CommandSource source)
        {
            Muted = false;
            return string.Empty;
        }

        string Peek(CommandSource source)
        {
            var peek = Data.Queue.PeekAhead();
            if (peek != null)
            {
                return string.Format("The next dog will be brought to you by {0}.", Bot.Names.GetFriendName(peek.Setter));
            }
            else
            {
                return "There are no more dogs! :(";
            }
        }

        string Sort(CommandSource source)
        {
            Data.Queue.Sort();
            return "Queue has been sorted.";
        }

        string QueueInfo(CommandSource source)
        {
            var users = Data.Queue.GetUsers();

            List<string> usersInfo = new List<string>();
            foreach (var user in users)
            {
                string name = Bot.Names.GetFriendName(user);
                int contributions = Data.Queue.GetUserContributions(user).Count;
                usersInfo.Add(string.Format("{0} ({1})", name, contributions));
            }

            return string.Format("Users in queue: " + string.Join(", ", usersInfo));
        }

        string MoveNext(CommandSource source)
        {
            Data.MoveToNextDog();

            if (Data.CurrentDog != null)
            {
                return string.Format("Dog of the Day is now: {0}", Data.CurrentDog.URL);
            }
            else
            {
                return string.Format("Queue is now empty.");
            }
        }

        string GetRandomDog(CommandSource source)
        {
            var rnd = new Random().Next(0, Data.HistoryStats.Dogs.Count);
            var dog = Data.HistoryStats.Dogs[rnd];
            return dog.URL;
        }

        string Post(CommandSource source)
        {
            CommandListener.FireCommand(DOTD);
            return string.Empty;
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
