using System.Collections.Generic;
using MemoryPack;

namespace Tarotro.Game.Data.Save {
    [MemoryPackable(GenerateType.VersionTolerant)]
    public partial class GameSaveData {
        [MemoryPackOrder(0)]
        public int Version { get; set; }

        [MemoryPackOrder(1)]
        public BattleSaveData Battle { get; set; }

        [MemoryPackOrder(2)]
        public int Score { get; set; }

        [MemoryPackOrder(3)]
        public RngSaveData Rng { get; set; }
    }

    [MemoryPackable(GenerateType.VersionTolerant)]
    public partial class BattleSaveData {
        [MemoryPackOrder(0)]
        public int CurrentCircleIndex { get; set; }

        [MemoryPackOrder(1)]
        public List<FightRoundSaveData> CurrentCircle { get; set; }

        [MemoryPackOrder(2)]
        public FightRoundSaveData CurrentRound { get; set; }

        [MemoryPackOrder(3)]
        public PlayerSaveData Player { get; set; }

        [MemoryPackOrder(4)]
        public EnemySaveData CurrentEnemy { get; set; }
    }

    [MemoryPackable(GenerateType.VersionTolerant)]
    public partial class FightRoundSaveData {
        [MemoryPackOrder(0)]
        public CircleType Circle { get; set; }

        [MemoryPackOrder(1)]
        public int CircleStep { get; set; }

        [MemoryPackOrder(2)]
        public EnemyType EnemyType { get; set; }

        [MemoryPackOrder(3)]
        public int TargetScore { get; set; }

        [MemoryPackOrder(4)]
        public string EnemyId { get; set; }
    }

    [MemoryPackable(GenerateType.VersionTolerant)]
    public partial class PlayerSaveData {
        [MemoryPackOrder(0)]
        public List<CardSaveData> Hand { get; set; }

        [MemoryPackOrder(1)]
        public DeckSaveData Deck { get; set; }
    }

    [MemoryPackable(GenerateType.VersionTolerant)]
    public partial class DeckSaveData {
        [MemoryPackOrder(0)]
        public List<CardSaveData> Cards { get; set; }

        [MemoryPackOrder(1)]
        public List<CardSaveData> DrawPile { get; set; }

        [MemoryPackOrder(2)]
        public List<CardSaveData> DiscardPile { get; set; }
    }

    [MemoryPackable(GenerateType.VersionTolerant)]
    public partial class CardSaveData {
        [MemoryPackOrder(0)]
        public string Id { get; set; }

        [MemoryPackOrder(1)]
        public string Name { get; set; }

        [MemoryPackOrder(2)]
        public int Damage { get; set; }

        [MemoryPackOrder(3)]
        public int Cost { get; set; }

        [MemoryPackOrder(4)]
        public string Description { get; set; }
    }

    [MemoryPackable(GenerateType.VersionTolerant)]
    public partial class EnemySaveData {
        [MemoryPackOrder(0)]
        public string EnemyId { get; set; }

        [MemoryPackOrder(1)]
        public int CurrentHealth { get; set; }
    }
}
