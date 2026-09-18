using NUnit.Framework;
using Tarotro.Game.Logic.Rng;

namespace Tarotro.Tests {
    [TestFixture]
    public class Pcg32Tests {
        [Test]
        public void SameSeed_ProducesSameSequence() {
            var a = new Pcg32(42, 1);
            var b = new Pcg32(42, 1);

            for (var i = 0; i < 100; i++) {
                Assert.AreEqual(a.Next(), b.Next(), $"Mismatch at step {i}");
            }
        }

        [Test]
        public void DifferentSeeds_ProduceDifferentSequences() {
            var a = new Pcg32(42, 1);
            var b = new Pcg32(99, 1);

            var same = true;
            for (var i = 0; i < 10; i++) {
                if (a.Next() != b.Next()) {
                    same = false;
                    break;
                }
            }

            Assert.IsFalse(same, "Different seeds produced identical sequences");
        }

        [Test]
        public void DifferentIncrements_ProduceDifferentSequences() {
            var a = new Pcg32(42, 1);
            var b = new Pcg32(42, 5);

            var same = true;
            for (var i = 0; i < 10; i++) {
                if (a.Next() != b.Next()) {
                    same = false;
                    break;
                }
            }

            Assert.IsFalse(same, "Different increments produced identical sequences");
        }

        [Test]
        public void Range_ReturnsValuesWithinBounds() {
            var rng = new Pcg32(12345, 1);

            for (var i = 0; i < 10000; i++) {
                var value = rng.Range(5, 10);
                Assert.GreaterOrEqual(value, 5);
                Assert.Less(value, 10);
            }
        }

        [Test]
        public void Range_EqualBounds_ReturnsMin() {
            var rng = new Pcg32(42, 1);
            Assert.AreEqual(7, rng.Range(7, 7));
        }

        [Test]
        public void Range_MaxLessThanMin_ReturnsMin() {
            var rng = new Pcg32(42, 1);
            Assert.AreEqual(10, rng.Range(10, 5));
        }

        [Test]
        public void Range_SingleValue_AlwaysReturnsThatValue() {
            var rng = new Pcg32(42, 1);

            for (var i = 0; i < 100; i++) {
                Assert.AreEqual(3, rng.Range(3, 4));
            }
        }

        [Test]
        public void NextFloat_ReturnsValuesBetweenZeroAndOne() {
            var rng = new Pcg32(42, 1);

            for (var i = 0; i < 10000; i++) {
                var value = rng.NextFloat();
                Assert.GreaterOrEqual(value, 0f);
                Assert.Less(value, 1f);
            }
        }

        [Test]
        public void NextFloat_HasReasonableDistribution() {
            var rng = new Pcg32(42, 1);
            var belowHalf = 0;
            var total = 10000;

            for (var i = 0; i < total; i++) {
                if (rng.NextFloat() < 0.5f) belowHalf++;
            }

            var ratio = belowHalf / (float)total;
            Assert.Greater(ratio, 0.45f, "Distribution too skewed high");
            Assert.Less(ratio, 0.55f, "Distribution too skewed low");
        }

        [Test]
        public void FromRawState_ContinuesSequence() {
            var rng = new Pcg32(42, 1);

            for (var i = 0; i < 50; i++) rng.Next();

            var savedState = rng.State;
            var savedIncrement = rng.Increment;

            var expected = new uint[10];
            for (var i = 0; i < 10; i++) expected[i] = rng.Next();

            var restored = Pcg32.FromRawState(savedState, savedIncrement);
            for (var i = 0; i < 10; i++) {
                Assert.AreEqual(expected[i], restored.Next(), $"Mismatch at step {i} after restore");
            }
        }

        [Test]
        public void Next_ProducesNonZeroValues() {
            var rng = new Pcg32(42, 1);
            var allZero = true;

            for (var i = 0; i < 100; i++) {
                if (rng.Next() != 0) {
                    allZero = false;
                    break;
                }
            }

            Assert.IsFalse(allZero, "Generator produced all zeros");
        }

        [Test]
        public void Range_CoversFullRange() {
            var rng = new Pcg32(42, 1);
            var seen = new bool[5];

            for (var i = 0; i < 10000; i++) {
                var value = rng.Range(0, 5);
                seen[value] = true;
            }

            for (var i = 0; i < 5; i++) {
                Assert.IsTrue(seen[i], $"Value {i} was never produced in Range(0, 5)");
            }
        }
    }
}
