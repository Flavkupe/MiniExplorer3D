using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class TiledSprite : MonoBehaviour {

    public Sprite Sprite = null;
    public GameObject Subrenderer = null;

	void Awake () 
    {
        if (this.Subrenderer == null)
        {
            this.Subrenderer = ResourceManager.GetSubrenderer();
        }

        Color[] baseColors = this.Sprite.texture.GetPixels();
        int textureWidth = this.Sprite.texture.width;
        int textureHeight = this.Sprite.texture.height;               
        SpriteRenderer spriteRenderer = this.GetComponent<SpriteRenderer>();
        int width = (int)(this.transform.localScale.x * spriteRenderer.sprite.bounds.size.x * spriteRenderer.sprite.pixelsPerUnit);
        int height = (int)(this.transform.localScale.y * spriteRenderer.sprite.bounds.size.y * spriteRenderer.sprite.pixelsPerUnit);

        Texture2D tiles = new Texture2D(width, height);
        for (int x = 0; x < width; x += textureWidth)
        {
            for (int y = 0; y < height; y += textureHeight)
            {
                int currentWidth = Mathf.Min(textureWidth, width - x - 1);
                int currentHeight = Mathf.Min(textureHeight, height - y - 1);
                
                if (currentWidth > 0 && currentHeight > 0)
                {
                    Color[] colors = baseColors;
                    if (currentWidth != textureWidth || currentHeight != textureHeight)
                    {
                        colors = this.Sprite.texture.GetPixels(0, 0, currentWidth, currentHeight);
                    }
                
                    tiles.SetPixels(x, y, currentWidth, currentHeight, colors);
                }
            
            }
        }
                   
        tiles.Apply();
        GameObject obj = GameObject.Instantiate(this.Subrenderer) as GameObject;
        SpriteRenderer subrenderer = obj.GetComponent<SpriteRenderer>();        
        subrenderer.sprite = Sprite.Create(tiles, new Rect(0, 0, tiles.width, tiles.height),
                                                     new Vector2(0.5f, 0.5f), 100);
        subrenderer.transform.parent = this.transform;
        subrenderer.transform.localPosition = new Vector3(0.0f, 0.0f, -0.5f);
        
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
