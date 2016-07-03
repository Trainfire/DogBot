using System;
using System.Timers;
using System.Collections.Generic;
using Core;
using SteamKit2;
using Modules.CommandHandler;

namespace Modules.DogOfTheDay
{
    public class DogOfTheDay : Module, ICommandListener
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
        const string STATS = "dotdstats";
        #endregion

        protected override void OnInitialize()
        {
            dogBotConfig = new Config();

            Data = new BotData();

            // Create the announcer
            announcer = new Announcer(dogBotConfig.Data.AnnouncementInterval);
            announcer.Announce += OnAnnounce;
            announcer.AllAnnounced += OnAllAnnouncements;

            var commandListener = Bot.GetOrAddModule<CommandListener>();
            commandListener.AddCommand<GetDogOfTheDay>(DOTD, this);
            commandListener.AddCommand<GetDogOfTheDayCount>(COUNT, this);
            commandListener.AddCommand<GetRandomDog>(RND, this);
            commandListener.AddCommand<Stats>(STATS, this);
            commandListener.AddCommand<SubmitDogOfTheDay>(DOTDSUBMIT, this);

            // Subscribe callbacks
            Bot.CallbackManager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnect);
            Bot.CallbackManager.Subscribe<SteamFriends.ChatEnterCallback>(OnJoinChat);
        }

        void ICommandListener.OnCommandTriggered(CommandEvent commandEvent)
        {
            ProcessCommand(commandEvent);   
        }

        void ProcessCommand(CommandEvent commandEvent)
        {
            var result = commandEvent.Command.Execute(commandEvent.Source);

            if (!string.IsNullOrEmpty(result.Message))
            {
                if (commandEvent.Source.Context == MessageContext.Chat)
                {
                    // say to chat here
                    Bot.SayToChat(Bot.CurrentChatRoomID, result.Message);
                }
                else
                {
                    // say to friend here
                    Bot.SayToFriend(commandEvent.Source.Caller, result.Message);
                }
            }
        }

        void OnDisconnect(SteamClient.DisconnectedCallback callback)
        {
            announcer.Stop();
        }

        void OnJoinChat(SteamFriends.ChatEnterCallback callback)
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
                Bot.GetModule<CommandListener>().FireCommand<GetDogOfTheDay>();
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
            public const string DogOfTheDay = "Dog of the Day";
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
