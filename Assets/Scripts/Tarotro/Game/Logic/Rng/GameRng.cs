using System;
using System.Collections.Generic;
using Tarotro.Game.Data.Save;
using Tarotro.Game.Utils;
using VContainer;

namespace Tarotro.Game.Logic.Rng {
    public class GameRng {
        private readonly Dictionary<RngChannel, Pcg32> _channels = new();
        private readonly IGameLogger _logger;
        private ulong _masterSeed;

        public ulong MasterSeed => _masterSeed;

        [Inject]
        public GameRng(IGameLogger logger) {
            _logger = logger;
        }

        public void Seed(ulong masterSeed) {
            _masterSeed = masterSeed;
            _channels.Clear();

            var channelValues = (RngChannel[])Enum.GetValues(typeof(RngChannel));
            for (var i = 0; i < channelValues.Length; i++) {
                var channel = channelValues[i];
                var channelSeed = MixSeed(masterSeed, (ulong)channel);
                var streamId = (ulong)channel * 2 + 1;
                _channels[channel] = new Pcg32(channelSeed, streamId);
            }

            _logger.Info($"RNG seeded with {masterSeed}", "Rng");
        }

        public void SeedFromTime() {
            var ticks = (ulong)DateTime.UtcNow.Ticks;
            Seed(ticks);
        }

        public int Range(RngChannel channel, int minInclusive, int maxExclusive) {
            var rng = _channels[channel];
            var result = rng.Range(minInclusive, maxExclusive);
            _channels[channel] = rng;
            return result;
        }

        public float NextFloat(RngChannel channel) {
            var rng = _channels[channel];
            var result = rng.NextFloat();
            _channels[channel] = rng;
            return result;
        }

        public uint Next(RngChannel channel) {
            var rng = _channels[channel];
            var result = rng.Next();
            _channels[channel] = rng;
            return result;
        }

        public RngSaveData CreateSaveSnapshot() {
            var channelStates = new List<RngChannelSaveData>();
            foreach (var kvp in _channels) {
                channelStates.Add(new RngChannelSaveData {
                    Channel = (int)kvp.Key,
                    State = kvp.Value.State,
                    Increment = kvp.Value.Increment
                });
            }

            return new RngSaveData {
                MasterSeed = _masterSeed,
                Channels = channelStates
            };
        }

        public void RestoreFromSave(RngSaveData saveData) {
            _masterSeed = saveData.MasterSeed;
            _channels.Clear();

            foreach (var channelData in saveData.Channels) {
                var channel = (RngChannel)channelData.Channel;
                _channels[channel] = Pcg32.FromRawState(channelData.State, channelData.Increment);
            }

            _logger.Info($"RNG restored from save (seed {_masterSeed})", "Rng");
        }

        private static ulong MixSeed(ulong seed, ulong channel) {
            var mixed = seed ^ (channel * 2654435761UL);
            mixed ^= mixed >> 30;
            mixed *= 0xbf58476d1ce4e5b9UL;
            mixed ^= mixed >> 27;
            mixed *= 0x94d049bb133111ebUL;
            mixed ^= mixed >> 31;
            return mixed;
        }
    }
}
