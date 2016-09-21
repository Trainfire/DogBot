using System;
using System.Linq;
using Core;
using DogBot.Extensions;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Modules.CatOfTheDay
{
    public enum AnnouncementMode
    {
        Hourly,
        Daily,
    }

    class Storage<T>
    {
        private string _fileName;
        private T _data;

        public T Data { get { return _data; } }

        public Storage(string filename)
        {
            _fileName = filename;
            Load();
        }

        public void Load()
        {
            if (File.Exists(Path))
            {
                using (var file = File.OpenText(Path))
                {
                    _data = JsonConvert.DeserializeObject<T>(file.ReadToEnd());
                }
            }
            else
            {
                _data = default(T);
            }
        }

        public void Save(T data)
        {
            using (var sw = File.CreateText(Path))
            {
                sw.Write(JsonConvert.SerializeObject(data, Formatting.Indented));
            }
        }

        string Path
        {
            get { return AppDomain.CurrentDomain.BaseDirectory + _fileName; }
        }
    }

    abstract class ThingOfTheDay : Module
    {
        private Storage<List<Content>> _storage;
        private IContentQueue _contents;
        private Announcer _announcer;

        private AnnouncementMode _announcementMode;

        protected abstract string ContentName { get; }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            _storage = new Storage<List<Content>>(ContentFileName);

            _contents = new ContentQueue(_storage.Data);
            _contents.ContentsChanged += OnContentsChanged;

            // Commands
            CommandListener.AddCommand(FormatCommand("submit"), Submit);
            CommandListener.AddCommand(FormatCommand("get"), Get);
            CommandListener.AddCommand(FormatCommand("rnd"), GetRandom);
            CommandListener.AddCommand(FormatCommand("sort"), Sort, true);
            CommandListener.AddCommand(FormatCommand("move"), Move, true);

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

        private void OnContentsChanged(object sender, List<Content> e)
        {
            Logger.Info("Contents changed.");
            _storage.Save(e);
        }

        void OnAnnounce(object sender, EventArgs e)
        {
            if (!Bot.Connection.Connected)
                return;

            if (_announcementMode == AnnouncementMode.Hourly)
            {
                Logger.Info("Moving to next content in queue...");
                _contents.Move();
            }

            if (_contents.Get() != null)
            {
                Logger.Info("Posting announcement...");
                CommandListener.FireCommand(FormatCommand("get"));
            }
        }

        void OnAllAnnouncements(object sender, EventArgs e)
        {
            Logger.Info("All announcements for current dog shown.");

            _contents.Move();

            if (_contents.Get() != null)
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

                if (_contents.HasURL(url))
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

                    _contents.Submit(content);

                    return ContentName + " was added to the queue!";
                }
            }

            return "Failed to add " + ContentName.ToLower();
        }

        string Get(CommandSource source)
        {
            var content = _contents.Get();

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
            var content = _contents.GetRandom();

            if (content != null)
                return string.Format("*miaow* {0}", content.URL);

            return "Nothing to show!";
        }

        string Move(CommandSource source)
        {
            _contents.Move();
            return "Content is now: " + Get(source);
        }

        string Sort(CommandSource source)
        {
            _contents.Sort();
            return "Queue has been sorted.";
        }
        #endregion
    }

    class CatOfTheDay : ThingOfTheDay
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
