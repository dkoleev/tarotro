using UnityEngine;

namespace Tarotro.Game.Logic.Motion
{
    [CreateAssetMenu(menuName = "Tarotro/Card Hand Config", fileName = "CardHandConfig")]
    public sealed class CardHandConfig : ScriptableObject
    {
        [Header("Hand Area")]
        [Tooltip("Logical rect where cards are laid out (X, Y, Width, Height).")]
        public float handX = 2f;
        public float handY = 8f;
        public float handWidth = 16f;
        public float handHeight = 2.5f;

        [Header("Card Size")]
        public float cardWidth = 1.2f;
        public float cardHeight = 1.8f;

        [Header("Hand Limits")]
        [Tooltip("Max cards before the layout compresses.")]
        public int softLimit = 8;

        [Header("Deck Position")]
        [Tooltip("Where cards spawn from (top-left corner in logical space).")]
        public float deckX = 0.5f;
        public float deckY = 4f;

        [Header("Discard Position")]
        [Tooltip("Where discarded cards fly to.")]
        public float discardX = 18f;
        public float discardY = 4f;

        [Header("Gizmo Display")]
        [Tooltip("Flip Y when drawing gizmos to match MoveableView convention.")]
        public bool flipY = true;

        public Transform2D HandArea => new Transform2D(handX, handY, handWidth, handHeight);
        public Transform2D DeckPosition => new Transform2D(deckX, deckY, cardWidth, cardHeight);
        public Transform2D DiscardPosition => new Transform2D(discardX, discardY, cardWidth, cardHeight);
    }
}
