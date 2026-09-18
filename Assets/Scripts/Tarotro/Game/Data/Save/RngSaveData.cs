using System.Collections.Generic;
using MemoryPack;

namespace Tarotro.Game.Data.Save {
    [MemoryPackable(GenerateType.VersionTolerant)]
    public partial class RngSaveData {
        [MemoryPackOrder(0)]
        public ulong MasterSeed { get; set; }

        [MemoryPackOrder(1)]
        public List<RngChannelSaveData> Channels { get; set; }
    }

    [MemoryPackable(GenerateType.VersionTolerant)]
    public partial class RngChannelSaveData {
        [MemoryPackOrder(0)]
        public int Channel { get; set; }

        [MemoryPackOrder(1)]
        public ulong State { get; set; }

        [MemoryPackOrder(2)]
        public ulong Increment { get; set; }
    }
}
