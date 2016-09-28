using Core;
using System;
using System.Threading.Tasks;

namespace Core
{
    /// <summary>
    /// Base class for an anonymous command executed via a Chat or Friend message.
    /// </summary>
    public class AnonCommandAsync : ChatCommand
    {
        bool adminOnly;

        public Func<CommandSource, Task<string>> Func { get; private set; }
        public override bool AdminOnly
        {
            get { return adminOnly; }
        }

        public override bool IsAsync
        {
            get
            {
                return true;
            }
        }

        public AnonCommandAsync(string alias, Func<CommandSource, Task<string>> func, bool adminOnly)
        {
            this.adminOnly = adminOnly;

            Alias = alias;
            Func = func;
        }

        public override string Execute(CommandSource source)
        {
            return "";
        }

        public override async Task<string> ExecuteAsync(CommandSource source)
        {
            //return await Task.FromResult(Func(source));
            return await Func(source);
        }
    }
}
