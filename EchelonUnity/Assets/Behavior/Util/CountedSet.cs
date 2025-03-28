using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class CountedSet<K>
{
    private Dictionary<K, int> CountSet { get; } = new Dictionary<K, int>();

    public IEnumerable<K> Items => CountSet.Keys.ToList();

    public void Add(K item)
    {
        if (CountSet.TryGetValue(item, out var count))
        {
            CountSet[item] = count + 1;
            Debug.Log($"{item} +1 -> {count + 1}");
        }
        else
        {
            CountSet[item] = 1;
            Debug.Log($"{item} +1 -> one");
        }

    }

    public void Remove(K item)
    {
        if (CountSet.TryGetValue(item, out var count))
        {
            if (count > 1)
            {
                CountSet[item] = count - 1;
                Debug.Log($"{item} -1 -> {count - 1}");
            }
            else
            {
                CountSet.Remove(item);
                Debug.Log($"{item} -1 -> gone");
            }
        }
        else
        {}
    }

    public void Purge(K item)
        => CountSet.Remove(item);
}