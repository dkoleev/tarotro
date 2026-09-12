using System.Collections.Generic;

namespace Game.Data {
    public class GameData {
        public Dictionary<string, TarotCardData> TarotCards;
        public Dictionary<string, EnemyData> Enemies;
        public BattleData Battle;
    }
}