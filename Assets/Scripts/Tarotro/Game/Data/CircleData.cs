using System;
using System.Collections.Generic;

namespace Tarotro.Game.Data {
    [Serializable]
    public class CircleData {
        public CircleType type;
        public int index;
        public string name;
        public List<EnemyType> steps;
        public List<string> enemies;
    }
}