using Core;
using System.Threading.Tasks;
using System;

namespace Core
{
    /// <summary>
    /// Base class for an anonymous command executed via a Chat or Friend message.
    /// </summary>
    public class AnonCommand : ChatCommand
    {
        bool adminOnly;

        public Func<CommandSource, string> Func { get; private set; }
        public override bool AdminOnly
        {
            get { return adminOnly; }
        }

        public AnonCommand(string alias, Func<CommandSource, string> func, bool adminOnly)
        {
            this.adminOnly = adminOnly;

            Alias = alias;
            Func = func;
        }

        public override string Execute(CommandSource source)
        {
            return Func(source);
        }

        public override async Task<string> ExecuteAsync(CommandSource source)
        {
            return await Task.FromResult(Func(source));
        }
    }
}
