using System.Linq;
using System.Collections.Generic;

public static class Extensions {
    public static T RandomOrDefault<T>(this IEnumerable<T> items) {
        var countable = new List<T>(items);
        if (countable.Count == 0) {
            return default(T);
        }
        return countable.RandomItem();
    }

    public static IEnumerable<T> RandomOrNone<T>(this ICollection<T> value, int count) {
        if (value.Count > 0) {
            for (int i = 0; i < count; i++) {
                yield return RandomItem(value);
            }
        }
    }

    public static T RandomItem<T>(this ICollection<T> pool) {
        if (pool.Count == 0) {
            throw new System.Exception("Collection is empty");
        }
        return pool.ElementAt(UnityEngine.Random.Range(0, pool.Count));
    }
}