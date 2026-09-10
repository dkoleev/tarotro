namespace Game.Data {
    public class EnemyData {
        public int MaxHealth;
        public string Name;

        public EnemyData(string name, int maxHealth) {
            MaxHealth = maxHealth;
            Name = name;
        }
    }
}