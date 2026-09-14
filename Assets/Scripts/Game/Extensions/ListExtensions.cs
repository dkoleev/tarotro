using System.Collections.Generic;
using UnityEngine;

namespace Game.Extensions {
    public static class ListExtensions {
        // Fisher-Yates shuffle
        public static void Shuffle<T>(this IList<T> list) {
            for (var i = list.Count - 1; i >= 1; i--) {
                var j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
