using System;
using System.Collections.Generic;
using System.Linq;
using SteamKit2;
using System.Threading.Tasks;

/// <summary>
/// This extension listens for commands sent via Chat or Private Messaging and relays a callback to any subscribed listeners.
/// </summary>
namespace Core
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

        public CommandEvent(MessageContext context, SteamID caller, MessageParser parser, ChatCommand command, bool hadPermission)
        {
            Command = command;
            Source = new CommandSource(context, caller, parser, hadPermission);
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
        public bool HadPermission { get; private set; }

        public CommandSource(MessageContext context, SteamID caller, MessageParser parser, bool hadPermission)
        {
            Context = context;
            Caller = caller;
            Parser = parser;
            HadPermission = hadPermission;
        }
    }

    /// <summary>
    /// Listens for commands sent via Chat or Private Messaging and relays a callback to any listeners.
    /// </summary>
    public class CommandListener
    {
        Dictionary<string, ChatCommand> commands;
        Bot bot;

        public bool Muted { get; set; }

        public CommandListener(Bot bot)
        {
            this.bot = bot;

            commands = new Dictionary<string, ChatCommand>();

            bot.Connection.CallbackManager.Subscribe<SteamFriends.ChatMsgCallback>(OnReceiveChatMessage);
            bot.Connection.CallbackManager.Subscribe<SteamFriends.FriendMsgCallback>(OnReceiveFriendMessage);
        }

        public void AddCommand<TCommand>(string alias, Action<TCommand> onAdd = null) where TCommand : ChatCommand
        {
            var command = Activator.CreateInstance<TCommand>();

            command.Initialize(bot);
            command.Alias = alias;

            if (!commands.ContainsKey(alias))
            {
                commands.Add(alias, command);

                if (onAdd != null)
                    onAdd(command);
            }
            else
            {
                bot.Logger.Error("Cannot add command '{0}' as that has already been registered.", command.GetType().Name);
            }
        }

        public void AddCommand(string alias, Func<CommandSource, string> func, bool adminOnly = false)
        {
            var command = new AnonCommand(alias, func, adminOnly);
            commands.Add(alias, command);
        }

        public void AddCommandAsync(string alias, Func<CommandSource, Task<string>> func, bool adminOnly = false)
        {
            var command = new AnonCommandAsync(alias, func, adminOnly);
            commands.Add(alias, command);
        }

        /// <summary>
        /// Internally triggers the command and sends the output to chat.
        /// </summary>
        /// <typeparam name="TCommand"></typeparam>
        public void FireCommand(string alias)
        {
            var command = GetCommand(alias);
            if (command != null)
                HandleMessage(MessageContext.Chat, bot.Connection.SID, alias);
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
            var parser = new MessageParser(message);

            if (parser.IsValid)
            {
                // Find a matching command using the parsed alias   
                var command = GetCommand(parser.Command) as ChatCommand;

                if (command != null)
                {
                    // Cache the name of the caller.
                    bot.Names.Cache(caller);

                    var requiresPermission = command.UsersOnly || command.AdminOnly;
                    var hasPermission = command.UsersOnly && (bot.Users.IsAdmin(caller) || bot.Users.IsUser(caller)) || command.AdminOnly && bot.Users.IsAdmin(caller);
                    var commandEvent = new CommandEvent(context, caller, parser, command, requiresPermission ? hasPermission : true);

                    FireCallbacks(commandEvent);
                }
            }
        }

        void FireCallbacks(CommandEvent commandEvent)
        {
            bot.Logger.Info("Executing command '{0}'. Called by '{1}' from '{2}'.", commandEvent.Command.Alias, bot.Names.GetFriendName(commandEvent.Source.Caller), commandEvent.Source.Context);

            var alias = commandEvent.Command.Alias;
            if (commands.ContainsKey(alias))
            {
                // Process the command.
                var parser = new CommandParser(bot);
                parser.NoPermissionMessage = commandEvent.Command.NoPermissionMessage;
                parser.Silent = Muted;
                parser.ProcessCommand(commandEvent);
            }
        }

        Command GetCommand(string alias)
        {
            if (commands.ContainsKey(alias))
                return commands[alias];
            return null;
        }
    }
}
