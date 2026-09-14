using System.Collections.Generic;

namespace Tarotro.Game.Data {
    public class GameData {
        public Dictionary<string, TarotCardData> TarotCards;
        public Dictionary<string, EnemyData> Enemies;
        public BattleData Battle;
    }
}