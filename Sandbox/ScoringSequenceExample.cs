using System.Collections.Generic;
using Tarotro.Motion;
using Tarotro.Sequencing;
using UnityEngine;

namespace Tarotro.Examples
{
    /// <summary>
    /// What the two systems buy you, in one method.
    ///
    /// <see cref="ScoreHand"/> below is ordinary top-to-bottom code with no coroutines,
    /// no async, no callbacks and no tween chains — yet it plays out over about four
    /// seconds on screen, in order, and stays correct if the player mashes buttons
    /// (the queue is blocked) or the window loses focus (the clocks are unscaled).
    ///
    /// Compare with the alternative: a coroutine with yields interleaved into the
    /// scoring maths. That version cannot be unit-tested without a PlayMode harness,
    /// and every new joker trigger means another yield in the middle of your rules.
    /// Here the rules resolve synchronously and only the *presentation* is queued.
    /// </summary>
    public sealed class ScoringSequenceExample
    {
        private readonly EventQueue _queue;
        private readonly ScoreDisplay _display;

        public ScoringSequenceExample(EventQueue queue, ScoreDisplay display)
        {
            _queue = queue;
            _display = display;
        }

        /// <summary>
        /// Resolve and present one played hand.
        ///
        /// Note the split, which is the whole point:
        ///   - rules run NOW, synchronously, so the result is deterministic and testable
        ///   - presentation is enqueued and plays out later
        /// The local variables captured by the closures are already final values.
        /// </summary>
        public void ScoreHand(
            IReadOnlyList<ScoredCard> scoringCards,
            IReadOnlyList<JokerTrigger> jokers,
            HandType hand)
        {
            // ---- rules: synchronous, allocation-light, unit-testable ----------
            float chips = hand.BaseChips;
            float mult = hand.BaseMult;

            _queue.Wait(0.2f);

            // Announce the hand.
            _queue.Do(() => _display.SetHandName(hand.Name, hand.Level));
            _queue.Wait(0.3f);

            // ---- per card ----------------------------------------------------
            foreach (var card in scoringCards)
            {
                // Retriggers are resolved as a list first, then replayed. This is the
                // structure from evaluate_play and it is why seals, Hanging Chad and
                // "retrigger all" jokers compose without special cases.
                int repetitions = 1 + card.ExtraRetriggers;

                for (int r = 0; r < repetitions; r++)
                {
                    float cardChips = card.Chips;
                    chips += cardChips;
                    float chipsNow = chips;   // capture the value, not the variable

                    var view = card.View;
                    _queue.Add(GameEvent.After(0.08f, () =>
                    {
                        view.JuiceUp(0.8f, 0.4f);           // pop the card
                        _display.SetChips(chipsNow);         // number slams up
                        _display.ChipCounter.JuiceUp(0.5f, 0f);
                        Sfx.PlayChip(pitch: 0.9f + 0.05f * r);
                    }));
                }
            }

            // ---- jokers ------------------------------------------------------
            foreach (var joker in jokers)
            {
                var result = joker.Evaluate(hand, scoringCards);
                if (result.IsEmpty) continue;

                if (result.PlusMult != 0f) mult += result.PlusMult;
                if (result.TimesMult != 1f) mult *= result.TimesMult;
                if (result.PlusChips != 0f) chips += result.PlusChips;

                float multNow = mult;
                float chipsNow = chips;
                var view = joker.View;
                string label = result.Label;

                _queue.Add(GameEvent.After(0.12f, () =>
                {
                    view.JuiceUp(1.0f);                      // bigger pop for jokers
                    _display.SetMult(multNow);
                    _display.SetChips(chipsNow);
                    _display.ShowFloatingText(view, label);
                    Sfx.PlayJoker();
                }));
            }

            // ---- the slam ----------------------------------------------------
            float total = chips * mult;

            _queue.Wait(0.3f);

            // Ease is the right trigger here: the counter rolls up over 0.8 s while the
            // queue stays blocked, so nothing lands on top of it. EaseInt floors the
            // intermediate values so the player never sees 41283.7 chips.
            _queue.Add(GameEvent.EaseInt(
                get: () => _display.Score,
                set: v => _display.Score = v,
                to: _display.Score + total,
                duration: 0.8f));

            _queue.Do(() =>
            {
                _display.ScoreCounter.JuiceUp(1.2f, 0f);
                Sfx.PlayScoreSlam();
            });

            // A condition event: hold the queue until the card-fly-away animation
            // reports done, with no timeout and no polling coroutine.
            _queue.Add(GameEvent.Until(() => _display.AllCardsSettled));

            _queue.Wait(0.4f);
        }

        /// <summary>
        /// A second pattern worth copying: staggered reveal. The delay lives in the
        /// events, not in the loop, so this returns immediately.
        /// </summary>
        public void DealHand(IReadOnlyList<Moveable> cards, Transform2D deck)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                var card = cards[i];
                card.HardSet(deck);   // start at the deck

                _queue.Add(GameEvent.After(0.06f, () =>
                {
                    // No tween. Layout writes T next frame and the integrator flies
                    // the card over — including the lean from its own horizontal speed.
                    card.JuiceUp(0.3f);
                    Sfx.PlayCardDraw();
                }));
            }
        }
    }

    // ---------------------------------------------------------------------
    // Minimal stand-ins so the example compiles on its own. Replace with your types.
    // ---------------------------------------------------------------------

    public sealed class HandType
    {
        public string Name;
        public int Level;
        public float BaseChips;
        public float BaseMult;
    }

    public sealed class ScoredCard
    {
        public float Chips;
        public int ExtraRetriggers;
        public Moveable View;
    }

    public readonly struct JokerResult
    {
        public readonly float PlusChips;
        public readonly float PlusMult;
        public readonly float TimesMult;
        public readonly string Label;

        public JokerResult(float plusChips, float plusMult, float timesMult, string label)
        {
            PlusChips = plusChips;
            PlusMult = plusMult;
            TimesMult = timesMult;
            Label = label;
        }

        public bool IsEmpty => PlusChips == 0f && PlusMult == 0f && TimesMult == 1f;
        public static JokerResult None => new JokerResult(0f, 0f, 1f, null);
    }

    public abstract class JokerTrigger
    {
        public Moveable View;
        public abstract JokerResult Evaluate(HandType hand, IReadOnlyList<ScoredCard> cards);
    }

    public sealed class ScoreDisplay
    {
        public float Score;
        public Moveable ChipCounter = new Moveable();
        public Moveable ScoreCounter = new Moveable();
        public bool AllCardsSettled = true;

        public void SetHandName(string name, int level) { }
        public void SetChips(float chips) { }
        public void SetMult(float mult) { }
        public void ShowFloatingText(Moveable at, string text) { }
    }

    internal static class Sfx
    {
        public static void PlayChip(float pitch) { }
        public static void PlayJoker() { }
        public static void PlayScoreSlam() { }
        public static void PlayCardDraw() { }
    }
}
