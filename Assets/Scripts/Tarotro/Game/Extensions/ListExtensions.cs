using System.Collections.Generic;
using Tarotro.Game.Logic.Rng;

namespace Tarotro.Game.Extensions {
    public static class ListExtensions {
        public static void Shuffle<T>(this IList<T> list, GameRng rng, RngChannel channel) {
            for (var i = list.Count - 1; i >= 1; i--) {
                var j = rng.Range(channel, 0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
