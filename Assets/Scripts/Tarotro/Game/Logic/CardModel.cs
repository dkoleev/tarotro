using System;
using Tarotro.Game.Data;

namespace Tarotro.Game.Logic {
    public class CardModel {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Damage { get; set; }
        public int Cost { get; set; }
        public string Description { get; set; }
        public string SpritePath { get; set; }

        public CardModel() {
            Id = Guid.NewGuid().ToString();
        }

        public CardModel(string id, string name, int damage, int cost, string description) {
            Id = id;
            Name = name;
            Damage = damage;
            Cost = cost;
            Description = description;
        }

        public CardModel(TarotCardData data) {
            Id = Guid.NewGuid().ToString();
            Name = data.nameLoc;
            Damage = data.value;
            SpritePath = data.spritePath;
        }
    }
}
