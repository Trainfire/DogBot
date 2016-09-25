using System;
using Core;
using System.Collections.Generic;

namespace Modules.ThingOfTheDay
{
    public enum AnnouncementMode
    {
        Hourly,
        Daily,
    }

    abstract class ThingOfTheDay : Module
    {
        private Storage<List<Content>> _content;
        private Storage<List<Content>> _history;

        private IContentQueue _queue;
        private Announcer _announcer;

        private AnnouncementMode _announcementMode;

        protected abstract string ContentName { get; }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            // Storage
            _content = new Storage<List<Content>>(ContentFileName);
            _history = new Storage<List<Content>>(ContentName.ToLower() + "s.history.json");

            // Queue
            _queue = new ContentQueue(_content.Data);
            _queue.ContentsChanged += OnContentChanged;
            _queue.ContentMoved += OnContentMoved;

            // Commands
            CommandListener.AddCommand(FormatCommand("submit"), Submit);
            CommandListener.AddCommand(FormatCommand("get"), Get);
            CommandListener.AddCommand(FormatCommand("rnd"), GetRandom);
            CommandListener.AddCommand(FormatCommand("sort"), Sort, true);
            CommandListener.AddCommand(FormatCommand("move"), Move, true);
            CommandListener.AddCommand(FormatCommand("stats"), Stats);

            // Create the announcer
            _announcer = new Announcer();
            _announcer.Announce += OnAnnounce;
            _announcer.AllAnnounced += OnAllAnnouncements;

            // Make an announcement once every hour.
            for (int i = 0; i < 24; i++)
            {
                _announcer.AddTime(i, 59, 59);
            }

            _announcer.Start();
        }

        void OnContentMoved(object sender, ContentMoveEvent e)
        {
            if (e.Old != null)
            {
                if (_history.Data == null)
                {
                    var data = new List<Content>();
                    data.Add(e.Old);
                    _history.Save(data);
                }
                else
                {
                    _history.Data.Add(e.Old);
                    _history.Save();
                }
            }
        }

        void OnContentChanged(object sender, List<Content> e)
        {
            Logger.Info("Contents changed.");
            _content.Save(e);
        }

        void OnAnnounce(object sender, EventArgs e)
        {
            if (!Bot.Connection.Connected)
                return;

            if (_announcementMode == AnnouncementMode.Hourly)
            {
                Logger.Info("Moving to next content in queue...");
                _queue.Move();
            }

            if (_queue.Get() != null)
            {
                Logger.Info("Posting announcement...");
                CommandListener.FireCommand(FormatCommand("get"));
            }
        }

        void OnAllAnnouncements(object sender, EventArgs e)
        {
            Logger.Info("All announcements for current dog shown.");

            _queue.Move();

            if (_queue.Get() != null)
            {
                Logger.Info("Moving to next content in queue...");
            }
            else
            {
                Logger.Info("Nothing to announce.");
            }
        }

        string FormatCommand(string command)
        {
            return "!" + ContentName.ToLower() + command;
        }

        string ContentFileName
        {
            get { return ContentName.ToLower() + "s.json"; }
        }

        #region Commands
        string Submit(CommandSource source)
        {
            if (source.Parser.Args.Count != 0)
            {
                var url = source.Parser.Args[0];

                if (_queue.HasURL(url))
                {
                    return ContentName + " could not be added as that URL already exists!";
                }
                else
                {
                    var content = new Content()
                    {
                        Setter = source.Caller.AccountID,
                        URL = url,
                    };

                    _queue.Submit(content);

                    return ContentName + " was added to the queue!";
                }
            }

            return "Failed to add " + ContentName.ToLower();
        }

        string Get(CommandSource source)
        {
            var content = _queue.Get();

            if (content != null)
            {
                return string.Format("{0} of the Day // {1} // Courtesy of {2}", ContentName, content.URL, Bot.Names.GetFriendName(content.Setter));
            }
            else
            {
                return "The queue is empty!";
            }
        }

        string GetRandom(CommandSource source)
        {
            var content = _queue.GetRandom();

            if (content != null)
                return string.Format("*miaow* {0}", content.URL);

            return "Nothing to show!";
        }

        string Move(CommandSource source)
        {
            _queue.Move();
            return "Content is now: " + Get(source);
        }

        string Sort(CommandSource source)
        {
            _queue.Sort();
            return "Queue has been sorted.";
        }

        string Stats(CommandSource source)
        {
            var statsHelper = new StatsHelper(_queue.Contents, _history.Data);

            var str = new List<string>();

            // Total records
            str.Add(ContentName + "s added to this day: " + statsHelper.TotalSubmissions);

            // Dogs Shown
            str.Add(ContentName + "s shown: " + statsHelper.TotalShown);

            // Highest contributor
            var highestContributor = statsHelper.HighestContributor;
            if (highestContributor != null)
            {
                var name = Bot.Names.GetFriendName(highestContributor);
                str.Add(string.Format("{0}{1} ({2})", "Highest contributor: ", name, statsHelper.GetUserContributions(highestContributor).Count));
            }

            // Include caller's contributions if they have any.
            var callerContributions = statsHelper.GetUserContributions(source.Caller);
            if (callerContributions.Count != 0)
            {
                var callerName = Bot.Names.GetFriendName(source.Caller);
                str.Add(string.Format("{0}'s total submissions: {1}", callerName, callerContributions.Count));
            }

            return string.Join(" // ", str.ToArray());
        }
        #endregion
    }
}

namespace Modules.CatOfTheDay
{
    class CatOfTheDay : ThingOfTheDay.ThingOfTheDay
    {
        protected override string ContentName
        {
            get
            {
                return "Cat";
            }
        }
    }
}
