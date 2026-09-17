using System.Collections.Generic;
using Tarotro.Game.Extensions;

namespace Tarotro.Game.Logic {
    public class Deck {
        private List<CardModel> _cards;
        private List<CardModel> _drawPile;
        private List<CardModel> _discardPile;

        public Deck() {
            _cards = new List<CardModel>();
            _drawPile = new List<CardModel>();
            _discardPile = new List<CardModel>();
        }

        public void AddCard(CardModel card) {
            if (card != null) {
                _cards.Add(card);
            }
        }

        public void AddCards(IEnumerable<CardModel> cards) {
            if (cards != null) {
                foreach (var card in cards) {
                    AddCard(card);
                }
            }
        }

        public CardModel Draw() {
            if (_drawPile.Count > 0) {
                var card = _drawPile[0];
                _drawPile.RemoveAt(0);
                return card;
            }

            // If no cards in draw pile, reshuffle discard pile
            if (_discardPile.Count > 0) {
                Reshuffle();
                return Draw();
            }

            return null;
        }

        public void Discard(CardModel card) {
            if (card != null) {
                _discardPile.Add(card);
            }
        }

        public void DiscardHand(IEnumerable<CardModel> cards) {
            if (cards != null) {
                foreach (var card in cards) {
                    Discard(card);
                }
            }
        }

        public void Shuffle() {
            _cards.Shuffle();
        }

        public void Reshuffle() {
            // Move all discarded cards to draw pile and shuffle
            _drawPile.AddRange(_discardPile);
            _discardPile.Clear();

            Shuffle();
        }

        public bool IsEmpty() {
            return _cards.Count == 0 && _drawPile.Count == 0;
        }

        public int Size() {
            return _cards.Count + _drawPile.Count + _discardPile.Count;
        }
    }
}
