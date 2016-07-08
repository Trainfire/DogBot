using Core;

namespace Modules.ChatJoiner
{
    class Config : FileStorage<ConfigData> { }

    public class ConfigData
    {
        /// <summary>
        /// Which chat room to connect to.
        /// </summary>
        public string ChatRoomId { get; set; }

        /// <summary>
        /// How long to wait for no activity before automatically rejoining chat.
        /// </summary>
        public double RejoinInterval { get; set; }

        /// <summary>
        /// How long to wait between leaving and rejoining chat.
        /// </summary>
        public double RejoinIntermission { get; set; }
    }
}
