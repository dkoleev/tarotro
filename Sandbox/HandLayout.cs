using System.Collections.Generic;
using UnityEngine;

namespace Tarotro.Motion
{
    /// <summary>
    /// Port of the hand branch of CardArea:align_cards (cardarea.lua:410).
    ///
    /// Included because it is the clearest demonstration of the T/VT contract: this
    /// runs every frame and writes only targets. It never tweens, never checks whether
    /// a card is already moving, and never cares that a card was just dragged — the
    /// integrator sorts that out. Adding or removing a card is one call.
    /// </summary>
    public static class HandLayout
    {
        /// <summary>Fan rotation across the hand, in radians at the edges. Source: 0.2</summary>
        public const float FanRotation = 0.2f;

        /// <summary>Stronger fan used for booster-pack selection. Source: 0.4</summary>
        public const float FanRotationPack = 0.4f;

        /// <summary>Idle sway amplitude and frequency. Source: 0.02 * sin(2t), 0.03 * sin(0.666t)</summary>
        public const float IdleRotationAmount = 0.02f;
        public const float IdleRotationFrequency = 2f;
        public const float IdleBobAmount = 0.03f;
        public const float IdleBobFrequency = 0.666f;

        /// <summary>How far a selected card lifts. Source: G.HIGHLIGHT_H = 0.2 * CARD_H</summary>
        public const float HighlightLift = 0.2f;

        /// <summary>
        /// Writes T for every card in the hand.
        /// </summary>
        /// <param name="area">Bounds of the hand region, in game units.</param>
        /// <param name="cards">Cards, in hand order.</param>
        /// <param name="isHighlighted">Which cards are currently selected.</param>
        /// <param name="cardWidth">Nominal card width — not the per-card VT width.</param>
        /// <param name="softLimit">
        /// Hand size the spacing is computed against. Keep this at the max hand size so
        /// cards do not visibly respace every time one is played.
        /// </param>
        /// <param name="realTime">Wall clock, for the idle sway.</param>
        /// <param name="reducedMotion">Suppresses the sway.</param>
        /// <param name="fanStrength">FanRotation, or FanRotationPack in booster packs.</param>
        public static void Apply(
            Transform2D area,
            IReadOnlyList<Moveable> cards,
            IReadOnlyList<bool> isHighlighted,
            float cardWidth,
            int softLimit,
            float realTime,
            bool reducedMotion = false,
            float fanStrength = FanRotation)
        {
            int n = cards.Count;
            if (n == 0) return;

            int maxCards = Mathf.Max(n, softLimit);
            float span = Mathf.Max(maxCards - 1, 1);
            float sway = reducedMotion ? 0f : 1f;

            for (int k = 0; k < n; k++)
            {
                var card = cards[k];

                // 1-based in the source; keep the same maths and shift here.
                float i = k + 1;

                // Fan: linear in index, centred on the hand.
                float fan = fanStrength * (-n / 2f - 0.5f + i) / n;

                float x = area.X
                          + (area.W - cardWidth) * ((i - 1) / span - 0.5f * (n - maxCards) / span)
                          + 0.5f * (cardWidth - card.T.W);

                // Idle sway is phase-offset BY X, not by index. That is why the hand
                // ripples instead of pulsing in unison. Cheap, and it sells "alive".
                float rot = fan + sway * IdleRotationAmount * Mathf.Sin(IdleRotationFrequency * realTime + x);

                float lift = (isHighlighted != null && k < isHighlighted.Count && isHighlighted[k])
                    ? HighlightLift * card.T.H
                    : 0f;

                // Arc: cards toward the edges sit slightly lower.
                float arc = Mathf.Abs(0.5f * (-n / 2f + i - 0.5f) / n) - 0.2f;

                float y = area.Y + area.H * 0.5f - card.T.H * 0.5f
                          - lift
                          + sway * IdleBobAmount * Mathf.Sin(IdleBobFrequency * realTime + x)
                          + arc;

                card.T.X = x + card.ShadowParallax / 30f;
                card.T.Y = y;
                card.T.R = rot;
            }
        }

        /// <summary>
        /// Source sorts the backing list by on-screen centre every frame, so dragging a
        /// card left of its neighbour reorders the hand for free. Call after Apply.
        /// </summary>
        public static void SortByScreenOrder(List<Moveable> cards)
            => cards.Sort((a, b) => (a.VT.X + a.VT.W * 0.5f).CompareTo(b.VT.X + b.VT.W * 0.5f));
    }
}
