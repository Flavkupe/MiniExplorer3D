

using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
public class WebRequestor
{
    public string JsonResult { get; private set; }

    public string Error { get; private set; } = null;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="apiUrl"></param>
    /// <returns></returns>
    public IEnumerator MakeWebRequest(string url, bool skipCache = false)
    {
        this.JsonResult = string.Empty;
        byte[] cachedData;
        if (!skipCache && SimpleCache.TryGetCached(url, out cachedData))
        {
            // Parse JSON and extract HTML
            this.JsonResult = Encoding.UTF8.GetString(cachedData);
            yield break;
        }
        else
        {
            using (UnityWebRequest uwr = UnityWebRequest.Get(url))
            {
                Debug.Log($"Sending request to {url}");
                yield return uwr.SendWebRequest();
                if (uwr.result != UnityWebRequest.Result.Success)
                {
                    this.Error = $"Page load error: {uwr.error} for {url}";
                    Debug.LogError(this.Error);
                    yield break;
                }
                else
                {
                    this.JsonResult = uwr.downloadHandler.text;
                    if (!skipCache)
                    {
                        SimpleCache.SaveToCache(url, Encoding.UTF8.GetBytes(this.JsonResult));
                    }
                }
            }
        }

        yield break;
    }

}