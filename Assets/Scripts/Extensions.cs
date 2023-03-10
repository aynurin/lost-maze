using System.Linq;
using System.Collections.Generic;

public static class Extensions {
    private static int seed = 1;
    public static T RandomOrNull<T>(this IEnumerable<T> items) {
        var countable = new List<T>(items);
        if (countable.Count == 0) {
            return default(T);
        }
        return countable.RandomItem();
    }

    public static T RandomItem<T>(this ICollection<T> pool) {
        // UnityEngine.Random.InitState(++seed);
        return pool.ElementAt(UnityEngine.Random.Range(0, pool.Count));
    }

    public static IEnumerable<T> RandomItems<T>(this ICollection<T> value, int count) {
        for (int i = 0; i < count; i++) {
            yield return RandomItem(value);
        }
    }

    public static IList<T> RandomItems<T>(this T[] value, int count) {
        List<T> vals = new List<T>();
        for (int i = 0; i < count; i++) {
            vals.Add(RandomItem(value));
        }
        return vals;
    }
}