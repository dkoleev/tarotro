using System.Collections.Generic;

namespace Tarotro.Game.Logic {
    public class PlayerModel {
        public List<CardModel> Hand => _hand;
        public Deck Deck => _deck;

        private List<CardModel> _hand;
        private Deck _deck;

        public PlayerModel() {
            _hand = new List<CardModel>();
            _deck = new Deck();
        }

        public PlayerModel(List<CardModel> hand, Deck deck) {
            _hand = hand;
            _deck = deck;
        }

        public int PlayHand() {
            // Calculate damage based on cards in hand
            var totalDamage = 0;
            foreach (var card in _hand) {
                if (card != null) {
                    totalDamage += card.Damage; // assuming CardModel has a Damage property
                }
            }

            // Clear the hand after playing
            _hand.Clear();

            return totalDamage;
        }

        public void DiscardHand() {
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
