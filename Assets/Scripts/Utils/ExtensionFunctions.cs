using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEditor.Build;
using UnityEngine;

public static class ExtensionFunctions
{
    private static System.Random rand = new System.Random();

    public static T GetRandom<T>(this IEnumerable<T> list)
    {
        List<T> items = list.ToList();

        if (items.Count == 0)
        {
            return default(T);
        }

        int index = rand.Next(0, items.Count);
        return items[index];
    }

    public static Queue<T> ToQueue<T>(this IEnumerable<T> list)
    {
        Queue<T> queue = new Queue<T>();
        foreach (T item in list)
        {
            queue.Enqueue(item);
        }

        return queue;
    }

    public static void TransferOneFrom<T>(this Queue<T> me, Queue<T> other)
    {
        if (other.Count > 0)
        {
            me.Enqueue(other.Dequeue());
        }
    }

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rand.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static void EnqueueRange<T>(this Queue<T> queue, IEnumerable<T> list)
    {
        foreach (T item in list)
        {
            queue.Enqueue(item);
        }
    }

    public static Vector2 SetX(this Vector2 vector, float x)
    {
        return new Vector2(x, vector.y);
    }

    public static Vector2 SetY(this Vector2 vector, float y)
    {
        return new Vector2(vector.x, y);
    }

    public static Vector3 SetX(this Vector3 vector, float x)
    {
        return new Vector3(x, vector.y, vector.z);
    }

    public static Vector3 SetY(this Vector3 vector, float y)
    {
        return new Vector3(vector.x, y, vector.z);
    }

    public static Vector3 SetZ(this Vector3 vector, float z)
    {
        return new Vector3(vector.x, vector.y, z);
    }

    public static R GetValueOrDefault<T, R>(this IDictionary<T, R> dict, T key)
    {
        if (dict.ContainsKey(key))
        {
            return dict[key];
        }

        return default(R);
    }

    public static bool IsSamePrefab(this IMatchesPrefab model, MonoBehaviour prefab)
    {
        if (model != null && prefab != null
            && string.Equals(model.PrefabID, prefab.name, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    public static void SetPixel(this Texture2D texture, int x, int y, Color color, int radius)
    {
        for (int i = -radius; i <= radius; ++i)
        {
            for (int j = -radius; j <= radius; ++j)
            {
                texture.SetPixel((int)x + i, (int)y + j, color);
            }
        }
    }

    public static float GetDistance(this Vector2 a, Vector2 b)
    {
        return Vector2.Distance(a, b);
    }

    public static bool IsCloseTo(this Vector2 a, Vector2 b, float threshold = 0.1f)
    {
        return a.GetDistance(b) < threshold;
    }

    public static bool IsPlayer(this GameObject obj)
    {
        return obj.transform.tag == "Player";
    }

    /// <summary>
    /// Perform a deep Copy of the object.
    /// </summary>
    /// <typeparam name="T">The type of object being copied.</typeparam>
    /// <param name="source">The object instance to copy.</param>
    /// <returns>The copied object.</returns>
    public static T DeepClone<T>(this T source)
    {
        if (!typeof(T).IsSerializable)
        {
            throw new ArgumentException("The type must be serializable.", "source");
        }

        // Don't serialize a null object, simply return the default for that object
        if (object.ReferenceEquals(source, null))
        {
            return default(T);
        }

        IFormatter formatter = new BinaryFormatter();
        Stream stream = new MemoryStream();
        using (stream)
        {
            formatter.Serialize(stream, source);
            stream.Seek(0, SeekOrigin.Begin);
            return (T)formatter.Deserialize(stream);
        }
    }

    // Returns all components of type T on the immediate children of a Transform
    public static IEnumerable<T> GetComponentsInDirectChildren<T>(this Transform parent) where T : Component
    {
        for (int i = 0; i < parent.childCount; ++i)
        {
            var child = parent.GetChild(i);
            var component = child.GetComponent<T>();
            if (component != null)
                yield return component;
        }
    }
}
