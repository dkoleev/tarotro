using System.Collections.Generic;
using Tarotro.Game.Logic.Rng;

namespace Tarotro.Game.Logic {
    public class PlayerModel {
        public List<CardModel> Hand => _hand;
        public Deck Deck => _deck;

        private List<CardModel> _hand;
        private Deck _deck;

        public PlayerModel(GameRng rng) {
            _hand = new List<CardModel>();
            _deck = new Deck(rng);
        }

        public PlayerModel(List<CardModel> hand, Deck deck) {
            _hand = hand;
            _deck = deck;
        }

        public int PlayHand() {
            var totalDamage = 0;
            foreach (var card in _hand) {
                if (card != null) {
                    totalDamage += card.Damage;
                }
            }

            _deck.DiscardHand(_hand);
            _hand.Clear();

            return totalDamage;
        }

        public void DiscardHand() {
            _deck.DiscardHand(_hand);
            _hand.Clear();
        }

        public void DrawCard() {
            if (_deck != null && !_deck.IsEmpty()) {
                var card = _deck.Draw();
                if (card != null) {
                    _hand.Add(card);
                }
            }
        }

        public void DrawHand(int count) {
            for (int i = 0; i < count; i++) {
                DrawCard();
            }
        }
    }
}
