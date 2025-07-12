using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ImagePanel : MonoBehaviour
{
    public RawImage RawImage; // Assign in Inspector
    public TextMeshProUGUI CaptionText; // Assign in Inspector

    public AspectRatioFitter AspectRatioFitter;

    /// <summary>
    /// Sets the image and caption for the panel, preserving aspect ratio.
    /// </summary>
    public void SetImage(LevelImage imageData)
    {
        if (RawImage == null || CaptionText == null || imageData == null)
        {
            return;
        }

        // Set caption
        CaptionText.text = imageData.Name ?? string.Empty;

        // Set image
        if (imageData.Texture2D == null)
        {
            RawImage.texture = null;
            RawImage.enabled = false;
        }
        else 
        {
            RawImage.texture = imageData.Texture2D;
            RawImage.enabled = true;

            // Preserve aspect ratio by adjusting RectTransform
            RectTransform rt = RawImage.rectTransform;
            float texWidth = imageData.Texture2D.width;
            float texHeight = imageData.Texture2D.height;
            // float panelWidth = rt.rect.width;
            // float panelHeight = rt.rect.height;
            float texAspect = texWidth / texHeight;

            AspectRatioFitter.aspectRatio = texAspect;

            // float panelAspect = panelWidth / panelHeight;

            //if (texAspect > panelAspect)
            //{
            //    // Fit width
            //    float height = panelWidth / texAspect;
            //    rt.sizeDelta = new Vector2(panelWidth, height);
            //}
            //else
            //{
            //    // Fit height
            //    float width = panelHeight * texAspect;
            //    rt.sizeDelta = new Vector2(width, panelHeight);
            //}
        }
    }
}