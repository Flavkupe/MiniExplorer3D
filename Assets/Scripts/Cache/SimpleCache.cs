using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class SimpleCache
{
    private static readonly string CacheDir = Path.Combine(Application.temporaryCachePath, "WikipediaMuseum");

    static SimpleCache()
    {
        if (!Directory.Exists(CacheDir))
            Directory.CreateDirectory(CacheDir);
    }

    private static string HashUrl(string url)
    {
        using (SHA256 sha = SHA256.Create())
        {
            byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(url));
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hash)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }

    public static string GetCachePath(string url)
    {
        return Path.Combine(CacheDir, HashUrl(url));
    }

    public static bool TryGetCached(string url, out byte[] data)
    {
        string path = GetCachePath(url);
        if (File.Exists(path))
        {
            Debug.Log($"Cache hit for URL: {url}");
            data = File.ReadAllBytes(path);
            return true;
        }

        Debug.Log($"Cache miss for URL: {url}");
        data = null;
        return false;
    }

    public static void SaveToCache(string url, byte[] data)
    {
        string path = GetCachePath(url);
        File.WriteAllBytes(path, data);
    }

    public static void ClearCache()
    {
        if (Directory.Exists(CacheDir))
        {
            Directory.Delete(CacheDir, true);
            Directory.CreateDirectory(CacheDir);
        }
    }
}
