using System.Collections.Generic;

namespace Core
{
    public class Strings
    {
        public virtual string NoPermission { get; } = "You do not have permission to do that!";
        public virtual string Muted { get; } = "*muted*";
        public virtual string Unmuted { get; } = "*unmuted*";
    }
}
