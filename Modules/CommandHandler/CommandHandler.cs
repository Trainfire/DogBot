using System;
using System.Collections.Generic;
using Core;
using SteamKit2;

/// <summary>
/// This module listens for commands sent via Chat or Private Messaging and relays a callback to any subscribed listeners.
/// </summary>
namespace Modules.CommandHandler
{
    public enum MessageContext
    {
        Chat,
        Friend,
    }

    public class CommandEvent
    {
        public ChatCommand Command { get; private set; }
        public CommandSource Source { get; private set; }

        public CommandEvent(CommandSource source, ChatCommand command)
        {
            Command = command;
            Source = source;
        }

        public CommandEvent(MessageContext context, SteamID caller, MessageParser parser, ChatCommand command)
        {
            Command = command;
            Source = new CommandSource(context, caller, parser);
        }
    }

    /// <summary>
    /// The source of a command.
    /// </summary>
    public class CommandSource
    {
        public MessageContext Context { get; private set; }
        public SteamID Caller { get; private set; }
        public MessageParser Parser { get; private set; }

        public CommandSource(MessageContext context, SteamID caller, MessageParser parser)
        {
            Context = context;
            Caller = caller;
            Parser = parser;
        }
    }

    /// <summary>
    /// Receives a callback when a command is triggered.
    /// </summary>
    public interface ICommandListener
    {
        void OnCommandTriggered(CommandEvent commandEvent);
    }

    /// <summary>
    /// Listens for commands sent via Chat or Private Messaging and relays a callback to any listeners.
    /// </summary>
    public class CommandListener : Module
    {
        Dictionary<Command, List<ICommandListener>> listeners;
        CommandRegistry commands;

        public bool Muted { get; set; }

        protected override void OnInitialize()
        {
            listeners = new Dictionary<Command, List<ICommandListener>>();
            commands = new CommandRegistry("!", "");

            Bot.CallbackManager.Subscribe<SteamFriends.ChatMsgCallback>(OnReceiveChatMessage);
            Bot.CallbackManager.Subscribe<SteamFriends.FriendMsgCallback>(OnReceiveFriendMessage);
        }

        public void AddCommand<TCommand>(string alias, ICommandListener listener = null) where TCommand : ChatCommand
        {
            var command = Activator.CreateInstance<TCommand>();

            command.Initialize(Bot);
            command.Alias = alias;

            commands.AddCommand(command);

            if (!listeners.ContainsKey(command))
            {
                listeners.Add(command, new List<ICommandListener>());

                if (listener != null)
                    Subscribe<TCommand>(listener);
            }
            else
            {
                Logger.Error("Cannot add command '{0}' as that has already been registered.", command.GetType().Name);
            }
        }

        public void Subscribe<TCommand>(ICommandListener listener) where TCommand : ChatCommand
        {
            var command = commands.GetCommand<TCommand>();
            if (command != null)
            {
                listeners[command].Add(listener);
            }
        }

        /// <summary>
        /// Internally triggers the command and sends the output to chat.
        /// </summary>
        /// <typeparam name="TCommand"></typeparam>
        public void FireCommand<TCommand>() where TCommand : ChatCommand
        {
            var command = commands.GetCommand<TCommand>();
            if (command != null)
                HandleMessage(MessageContext.Chat, Bot.SID, commands.Format(command.Alias));
        }

        void OnReceiveChatMessage(SteamFriends.ChatMsgCallback callback)
        {
            HandleMessage(MessageContext.Chat, callback.ChatterID, callback.Message);
        }

        void OnReceiveFriendMessage(SteamFriends.FriendMsgCallback callback)
        {
            HandleMessage(MessageContext.Friend, callback.Sender, callback.Message);
        }

        void HandleMessage(MessageContext context, SteamID caller, string message)
        {
            var parser = new MessageParser(commands.Token, message);

            if (parser.IsValid)
            {
                // Find a matching command using the parsed alias   
                var command = commands.GetCommand(parser.Command) as ChatCommand;

                if (command != null)
                {
                    // Cache the name of the caller.
                    Bot.CacheName(caller);

                    var commandEvent = new CommandEvent(context, caller, parser, command);

                    var requiresPermission = command.UsersOnly || command.AdminOnly;
                    var hasPermission = command.UsersOnly && Bot.IsUser(caller) || command.AdminOnly && Bot.IsAdmin(caller);

                    if (requiresPermission && hasPermission)
                    {
                        FireCallbacks(commandEvent);
                    }
                    else if (requiresPermission && !hasPermission)
                    {
                        // TODO: Figure out what to do if the caller has no permission.
                        //Record = new CommandRecord(null, caller, new CommandResult(bot.CoreStrings.NoPermission), parser);
                    }
                    else
                    {
                        FireCallbacks(commandEvent);
                    }
                }
            }
        }

        void FireCallbacks(CommandEvent commandEvent)
        {
            listeners[commandEvent.Command].ForEach(x => x.OnCommandTriggered(commandEvent));
        }
    }

    /// <summary>
    /// A command executed via a Chat or Friend message.
    /// </summary>
    public abstract class ChatCommand : Command
    {
        public Bot Bot { get; private set; }
        public CommandResult Result { get; private set; }

        public virtual void Initialize(Bot bot)
        {
            Bot = bot;
        }

        public virtual CommandResult Execute(CommandSource source)
        {
            return new CommandResult();
        }
    }
}