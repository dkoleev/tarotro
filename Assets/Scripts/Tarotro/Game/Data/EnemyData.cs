using System;

namespace Tarotro.Game.Data {
    [Serializable]
    public class EnemyData {
        public string id;
        public int circle; //circle on which this enemy available
        public EnemyType type;
        public int damage;
        public string nameLoc;
        public string prefabPath;
    }
}