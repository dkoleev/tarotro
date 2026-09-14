using System;

namespace Tarotro.Game.Data {
    [Serializable]
    public class EnemyData {
        public string id;
        public int maxHealth;
        public int damage;
        public string nameLoc;
        public string prefabPath;
    }
}