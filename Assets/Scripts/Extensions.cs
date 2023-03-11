using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions {
    public static T RandomOrNull<T>(this IEnumerable<T> items) {
        var countable = new List<T>(items);
        if (countable.Count == 0) {
            return default(T);
        }
        return countable.RandomItem();
    }

    public static T RandomOrNull<T>(this IEnumerable<GameObject> items) {
        var countable = new List<T>(items.Select(go => go.GetComponent<T>()));
        if (countable.Count == 0) {
            return default(T);
        }
        return countable.RandomItem();
    }

    public static T RandomItem<T>(this ICollection<T> pool) {
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

    public static bool NameLike(this GameObject obj, string prefix, int maxNameLengthDelta) {
        return obj.name.StartsWith(prefix) && obj.name.Length < prefix.Length + maxNameLengthDelta;
    }
}