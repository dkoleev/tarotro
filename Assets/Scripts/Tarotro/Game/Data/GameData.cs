using System.Collections.Generic;

namespace Tarotro.Game.Data {
    public class GameData {
        public Dictionary<string, TarotCardData> TarotCards;
        public Dictionary<string, CharacterData> Characters;
        public Dictionary<string, EnemyData> Enemies;
        public Dictionary<CircleType, CircleData> Circles;
        public BattleData Battle;
    }
}