using System;

namespace Tarotro.Game.Data {
    [Serializable]
    public class BattleData {
        public int spreadDefaultSize;
        public int playHandSize;
        public BattleProgressionConfig progression;
    }
}