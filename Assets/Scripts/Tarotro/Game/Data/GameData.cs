using System.Collections.Generic;
using VContainer;

namespace Tarotro.Game.Data {
    public class GameData {
        [Inject]
        public GameData() { }

        public Dictionary<string, TarotCardData> TarotCards;
        public Dictionary<string, CharacterData> Characters;
        public Dictionary<string, EnemyData> Enemies;
        public Dictionary<CircleType, CircleData> Circles;
        public BattleData Battle;
    }
}