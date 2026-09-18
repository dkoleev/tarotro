using System.Runtime.CompilerServices;

namespace Tarotro.Game.Logic.Rng {
    public struct Pcg32 {
        private const ulong Multiplier = 6364136223846793005UL;

        private ulong _state;
        private ulong _increment;

        public ulong State => _state;
        public ulong Increment => _increment;

        public Pcg32(ulong state, ulong increment) {
            _increment = (increment << 1) | 1;
            _state = 0;
            Step();
            _state += state;
            Step();
        }

        public static Pcg32 FromRawState(ulong state, ulong increment) {
            var rng = default(Pcg32);
            rng._state = state;
            rng._increment = increment;
            return rng;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Next() {
            var oldState = _state;
            Step();
            return Rotr32(Xsh(oldState), Rot(oldState));
        }

        public int Range(int minInclusive, int maxExclusive) {
            if (maxExclusive <= minInclusive) return minInclusive;
            var range = (uint)(maxExclusive - minInclusive);
            return minInclusive + (int)(Next() % range);
        }

        public float NextFloat() {
            return (Next() >> 8) * (1.0f / 16777216.0f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Step() {
            _state = _state * Multiplier + _increment;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint Xsh(ulong state) {
            return (uint)(((state >> 18) ^ state) >> 27);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Rot(ulong state) {
            return (int)(state >> 59);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint Rotr32(uint value, int rot) {
            return (value >> rot) | (value << ((-rot) & 31));
        }
    }
}
