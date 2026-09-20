using Tarotro.Motion;
using TMPro;
using UnityEngine;

namespace Tarotro.Game.View {
    [RequireComponent(typeof(MoveableView))]
    public sealed class CardView : MonoBehaviour, ICardView {
        [SerializeField] private SpriteRenderer _cardSprite;
        [SerializeField] private SpriteRenderer _highlightBorder;
        [SerializeField] private TMP_Text _damageText;
        [SerializeField] private TMP_Text _nameText;

        private MoveableView _moveableView;

        public MoveableView MoveableView => _moveableView;

        private void Awake() {
            _moveableView = GetComponent<MoveableView>();
            if (_highlightBorder != null)
                _highlightBorder.enabled = false;
        }

        public void SetCardInfo(string cardName, int damage) {
            if (_nameText != null) _nameText.text = cardName;
            if (_damageText != null) _damageText.text = damage.ToString();
        }

        public void SetSprite(Sprite sprite) {
            if (_cardSprite != null) _cardSprite.sprite = sprite;
        }

        public void SetHighlighted(bool highlighted) {
            if (_highlightBorder != null)
                _highlightBorder.enabled = highlighted;
        }

        public void SetSelected(bool selected) {
            if (_cardSprite != null)
                _cardSprite.color = selected ? new Color(0.8f, 0.9f, 1f) : Color.white;
        }
    }
}
