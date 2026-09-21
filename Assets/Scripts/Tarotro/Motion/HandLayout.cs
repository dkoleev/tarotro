using System.Collections.Generic;
using UnityEngine;

namespace Tarotro.Motion
{
    public static class HandLayout
    {
        public const float FanRotation = 0.2f;
        public const float FanRotationPack = 0.4f;

        public const float IdleRotationAmount = 0.02f;
        public const float IdleRotationFrequency = 2f;
        public const float IdleBobAmount = 0.03f;
        public const float IdleBobFrequency = 0.666f;

        public const float HighlightLift = 0.2f;

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
                float i = k + 1;

                float fan = fanStrength * (-n / 2f - 0.5f + i) / n;

                float x = area.X
                          + (area.W - cardWidth) * ((i - 1) / span - 0.5f * (n - maxCards) / span)
                          + 0.5f * (cardWidth - card.T.W);

                float rot = fan + sway * IdleRotationAmount * Mathf.Sin(IdleRotationFrequency * realTime + x);

                float lift = (isHighlighted != null && k < isHighlighted.Count && isHighlighted[k])
                    ? HighlightLift * card.T.H
                    : 0f;

                float arc = Mathf.Abs(0.5f * (-n / 2f + i - 0.5f) / n) - 0.2f;

                float y = area.Y + area.H * 0.5f - card.T.H * 0.5f
                          - lift
                          + sway * IdleBobAmount * Mathf.Sin(IdleBobFrequency * realTime + x)
                          + arc;

                card.T.X = x;
                card.T.Y = y;
                card.T.R = rot;
            }
        }

        public static void SortByScreenOrder(List<Moveable> cards)
            => cards.Sort((a, b) => (a.VT.X + a.VT.W * 0.5f).CompareTo(b.VT.X + b.VT.W * 0.5f));
    }
}
