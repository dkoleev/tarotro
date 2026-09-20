using UnityEngine;

namespace Tarotro.Game.View {
    public interface ICardView {
        void SetCardInfo(string cardName, int damage);
        void SetSprite(Sprite sprite);
        void SetHighlighted(bool highlighted);
        void SetSelected(bool selected);
    }
}
