namespace Game.Data {
    public class EnemyData {
        public int MaxHealth;
        public string Name;

        public EnemyData(string mame, int maxHealth) {
            MaxHealth = maxHealth;
            Name = mame;
        }
    }
}