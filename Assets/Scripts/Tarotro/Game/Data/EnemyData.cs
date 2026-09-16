using System;

namespace Tarotro.Game.Data {
    [Serializable]
    public class EnemyData {
        public string id;
        public EnemyType type;
        public int damage;
    }
}