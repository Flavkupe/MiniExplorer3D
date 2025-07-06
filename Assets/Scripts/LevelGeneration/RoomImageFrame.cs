
using UnityEngine;
public class RoomImageFrame : MonoBehaviour, ICanLookAt
{
    public enum ImageFrameType
    {
        Painting,
        Podium
    }

    public ImageFrameType FrameType;

    public Simple3DText textObject;

    public bool IsUsed { get; set; }

    public float BaseWidthMultiplier = 1.0f;
    public float BaseHeightMultiplier = 1.0f;
    public float BasePPUMultiplier = 2.0f;

    void Start()
    {        
    }

    void Update()
    {
        if (this.textObject != null)
        {
            this.textObject.ToggleText(false);
        }
    }

    public void SetLevelImage(LevelImage newLevelImage)
    {
        if (newLevelImage == null || newLevelImage.Texture2D == null)
        {
            Debug.LogError("RoomImageFrame: SetLevelImage called with null or invalid LevelImage.");
            return;
        }

        if (this.GetComponent<Renderer>() != null && this.GetComponent<Renderer>() is SpriteRenderer)
        {
            this.gameObject.SetActive(true);
            SpriteRenderer spriteRenderer = this.GetComponent<Renderer>() as SpriteRenderer;
            Texture2D texture = newLevelImage.Texture2D;
            spriteRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width * BaseWidthMultiplier, texture.height * BaseWidthMultiplier), Vector2.zero, 100 * BasePPUMultiplier);
            this.IsUsed = true;
        }
        else if (this.GetComponentInChildren<MeshRenderer>() != null)
        {
            MeshRenderer meshRenderer = this.GetComponentInChildren<MeshRenderer>();
            int materialIndex = 0;
            if (meshRenderer.materials.Length > 1)
            {
                materialIndex = 1;                
            }

            meshRenderer.materials[materialIndex].SetTexture("_MainTex", newLevelImage.Texture2D);

            this.IsUsed = true;
        }

        if (this.textObject != null)
        {
            this.textObject.SetText(newLevelImage.Name);
        }
    }

    public void LookAt(GameObject source)
    {
        if (this.textObject != null)
        {
            this.textObject.ToggleText(true);
        }
    }
}

