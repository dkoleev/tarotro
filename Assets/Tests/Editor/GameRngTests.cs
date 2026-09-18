using System.Collections.Generic;
using NUnit.Framework;
using Tarotro.Game.Extensions;
using Tarotro.Game.Logic.Rng;

namespace Tarotro.Tests {
    [TestFixture]
    public class GameRngTests {
        private GameRng CreateRng(ulong seed) {
            var rng = new GameRng(new NullLogger());
            rng.Seed(seed);
            return rng;
        }

        [Test]
        public void SameSeed_ProducesSameResults() {
            var a = CreateRng(42);
            var b = CreateRng(42);

            for (var i = 0; i < 100; i++) {
                Assert.AreEqual(
                    a.Range(RngChannel.Shuffle, 0, 1000),
                    b.Range(RngChannel.Shuffle, 0, 1000),
                    $"Mismatch at step {i}"
                );
            }
        }

        [Test]
        public void DifferentSeeds_ProduceDifferentResults() {
            var a = CreateRng(42);
            var b = CreateRng(99);

            var same = true;
            for (var i = 0; i < 10; i++) {
                if (a.Range(RngChannel.Shuffle, 0, 100000) != b.Range(RngChannel.Shuffle, 0, 100000)) {
                    same = false;
                    break;
                }
            }

            Assert.IsFalse(same, "Different seeds produced identical sequences");
        }

        [Test]
        public void ChannelIsolation_DrawingFromOneDoesNotAffectAnother() {
            var rng = CreateRng(42);
            var control = CreateRng(42);

            // Draw heavily from EnemySelect on rng, but not on control
            for (var i = 0; i < 100; i++) {
                rng.Range(RngChannel.EnemySelect, 0, 100);
            }

            // Shuffle channel should still produce the same results on both
            for (var i = 0; i < 50; i++) {
                Assert.AreEqual(
                    control.Range(RngChannel.Shuffle, 0, 1000),
                    rng.Range(RngChannel.Shuffle, 0, 1000),
                    $"Channel isolation broken at step {i}"
                );
            }
        }

        [Test]
        public void AllChannels_ProduceDifferentStreams() {
            var rng = CreateRng(42);
            var firstValues = new Dictionary<RngChannel, uint>();

            foreach (RngChannel ch in System.Enum.GetValues(typeof(RngChannel))) {
                firstValues[ch] = rng.Next(ch);
            }

            var channels = new List<RngChannel>(firstValues.Keys);
            for (var i = 0; i < channels.Count; i++) {
                for (var j = i + 1; j < channels.Count; j++) {
                    Assert.AreNotEqual(
                        firstValues[channels[i]],
                        firstValues[channels[j]],
                        $"{channels[i]} and {channels[j]} produced the same first value"
                    );
                }
            }
        }

        [Test]
        public void SaveAndRestore_ContinuesIdentically() {
            var rng = CreateRng(42);

            // Advance state unevenly across channels
            for (var i = 0; i < 30; i++) rng.Range(RngChannel.Shuffle, 0, 52);
            for (var i = 0; i < 10; i++) rng.Range(RngChannel.EnemySelect, 0, 5);
            for (var i = 0; i < 5; i++) rng.NextFloat(RngChannel.BattleEvent);

            var snapshot = rng.CreateSaveSnapshot();

            // Record what the original produces next
            var expected = new int[20];
            for (var i = 0; i < 20; i++) {
                expected[i] = rng.Range(RngChannel.Shuffle, 0, 1000);
            }

            // Restore into a fresh instance
            var restored = new GameRng(new NullLogger());
            restored.RestoreFromSave(snapshot);

            for (var i = 0; i < 20; i++) {
                Assert.AreEqual(
                    expected[i],
                    restored.Range(RngChannel.Shuffle, 0, 1000),
                    $"Mismatch at step {i} after restore"
                );
            }
        }

        [Test]
        public void SaveAndRestore_PreservesMasterSeed() {
            var rng = CreateRng(12345);
            var snapshot = rng.CreateSaveSnapshot();

            var restored = new GameRng(new NullLogger());
            restored.RestoreFromSave(snapshot);

            Assert.AreEqual(12345UL, restored.MasterSeed);
        }

        [Test]
        public void SaveAndRestore_AllChannelsMatch() {
            var rng = CreateRng(42);

            // Advance each channel differently
            rng.Range(RngChannel.Shuffle, 0, 10);
            rng.Range(RngChannel.EnemySelect, 0, 10);
            rng.Range(RngChannel.EnemySelect, 0, 10);
            rng.NextFloat(RngChannel.BattleEvent);
            rng.Next(RngChannel.Reward);
            rng.Next(RngChannel.Reward);
            rng.Next(RngChannel.Reward);

            var snapshot = rng.CreateSaveSnapshot();

            var expected = new Dictionary<RngChannel, uint>();
            foreach (RngChannel ch in System.Enum.GetValues(typeof(RngChannel))) {
                expected[ch] = rng.Next(ch);
            }

            var restored = new GameRng(new NullLogger());
            restored.RestoreFromSave(snapshot);

            foreach (RngChannel ch in System.Enum.GetValues(typeof(RngChannel))) {
                Assert.AreEqual(expected[ch], restored.Next(ch), $"Channel {ch} mismatch after restore");
            }
        }

        [Test]
        public void Range_RespectsChannelBounds() {
            var rng = CreateRng(42);

            for (var i = 0; i < 10000; i++) {
                var value = rng.Range(RngChannel.EnemySelect, 3, 8);
                Assert.GreaterOrEqual(value, 3);
                Assert.Less(value, 8);
            }
        }

        [Test]
        public void NextFloat_RespectsChannelBounds() {
            var rng = CreateRng(42);

            for (var i = 0; i < 10000; i++) {
                var value = rng.NextFloat(RngChannel.BattleEvent);
                Assert.GreaterOrEqual(value, 0f);
                Assert.Less(value, 1f);
            }
        }

        [Test]
        public void Shuffle_IsDeterministic() {
            var rng1 = CreateRng(42);
            var rng2 = CreateRng(42);

            var list1 = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var list2 = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            list1.Shuffle(rng1, RngChannel.Shuffle);
            list2.Shuffle(rng2, RngChannel.Shuffle);

            Assert.AreEqual(list1, list2);
        }

        [Test]
        public void Shuffle_ActuallyShuffles() {
            var rng = CreateRng(42);
            var original = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var shuffled = new List<int>(original);

            shuffled.Shuffle(rng, RngChannel.Shuffle);

            Assert.AreNotEqual(original, shuffled, "Shuffle did not change the order");
            CollectionAssert.AreEquivalent(original, shuffled, "Shuffle lost or duplicated elements");
        }

        [Test]
        public void Shuffle_ChannelIsolation() {
            var rng = CreateRng(42);
            var control = CreateRng(42);

            // Shuffle a list on rng
            var list = new List<int> { 0, 1, 2, 3, 4, 5 };
            list.Shuffle(rng, RngChannel.Shuffle);

            // EnemySelect should be unaffected
            for (var i = 0; i < 20; i++) {
                Assert.AreEqual(
                    control.Range(RngChannel.EnemySelect, 0, 100),
                    rng.Range(RngChannel.EnemySelect, 0, 100),
                    $"Shuffle contaminated EnemySelect at step {i}"
                );
            }
        }
    }
}
