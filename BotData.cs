using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DogBot
{
    /// <summary>
    /// Information about the current state of the Bot.
    /// </summary>
    public class BotData
    {
        public DogData Dog { get; private set; }

        public BotData()
        {
            Dog = new DogData(null);
        }
    }
}
