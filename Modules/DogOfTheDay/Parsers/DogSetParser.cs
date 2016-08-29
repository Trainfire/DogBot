using Core;
using SteamKit2;
using System;
using System.Linq;

namespace Modules.DogOfTheDay
{
    public class DotdSetParser : MessageParser
    {
        public DogData Dog { get; private set; }

        public DotdSetParser(SteamID setter, string message) : base(message)
        {
            var url = "";
            var comment = "";

            if (Args.Count > 0 && IsURL(Args[0]))
            {
                url = Args[0];

                if (Args.Count > 1)
                {
                    comment = string.Join(" ", Args.Skip(1).ToList());
                }
            }
            else
            {
                Invalidate(DogOfTheDay.Strings.UrlInvalid);
            }

            if (IsValid)
            {
                Dog = new DogData();
                Dog.Setter = setter;
                Dog.URL = url;
                Dog.Message = !string.IsNullOrEmpty(comment) ? comment : "";
                Dog.TimeStamp = DateTime.UtcNow.ToBinary().ToString();
            }
        }

        bool IsURL(string message)
        {
            // TODO: Rubbish validation
            return message.StartsWith("http://") || message.StartsWith("https://") || message.StartsWith("www.");
        }
    }
}
