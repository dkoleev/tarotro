using System;

namespace Tarotro.Game.Data {
    [Serializable]
    public class EnemyData {
        public string id;
        public CircleType circle; //circle on which this enemy available
        public EnemyType type;
        public int damage;
        public string nameLoc;
        public string prefabPath;
    }
}