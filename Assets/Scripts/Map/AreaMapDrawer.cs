using UnityEngine;
using System.Collections;
using Assets.Scripts.LevelGeneration;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class AreaMapDrawer : MonoBehaviour 
{
	public GameObject Player = null;
	private Vector2? currentPlayerCoords;

	Color[,] baseColorMap;

	private Image image;

	void Awake() 
	{
		image = this.transform.GetComponent<Image>();
	}

	public void RefreshMinimap()
	{
		int dimensions = StageManager.RoomGridDimensions;
		Area area = StageManager.CurrentArea;
		RoomGrid grid = area.RoomGrid;
		Texture2D texture = new Texture2D(dimensions, dimensions);
		this.baseColorMap = new Color[dimensions, dimensions];
		for (int x = 0; x < dimensions; ++x)
		{
			for (int y = 0; y < dimensions; ++y)
			{
				if (grid.HasRoomAt(x, y))
				{
					texture.SetPixel(x, y, Color.red);
					this.baseColorMap[x, y] = Color.red;
				}
				else
				{
					texture.SetPixel(x, y, Color.white);
					this.baseColorMap[x, y] = Color.white;
				}
			}
		}

		texture.Apply();
			   
		this.image.sprite = Sprite.Create(texture, new Rect(0, 0, dimensions, dimensions),
											  new Vector2(dimensions / 2.0f, dimensions / 2.0f));
		
	}
		
	void FixedUpdate () 
	{
		if (this.Player != null)
		{
			Vector2 gridCoords = StageManager.GetGridCoordsFromWorldCoords(this.Player.transform.position);
			if (this.currentPlayerCoords == null ||
				gridCoords.x != currentPlayerCoords.Value.x ||
				gridCoords.y != currentPlayerCoords.Value.y)
			{                
				this.UpdatePlayerMapLocation(gridCoords, currentPlayerCoords);
				this.currentPlayerCoords = gridCoords;
			}

		}
		
	}

	private void UpdatePlayerMapLocation(Vector2 newCoords, Vector2? oldCoords)
	{
		int newX = (int)newCoords.x;
		int newY = (int)newCoords.y;

		Area area = StageManager.CurrentArea;
		if (area == null || area.RoomGrid == null)
		{
			return;
		}
		
		RoomGrid grid = area.RoomGrid;
		Texture2D texture = this.image.sprite.texture;

		if (oldCoords != null)
		{
			int oldXTopLeft = Mathf.Max(0, (int)oldCoords.Value.x - 1);
			int oldYTopLeft = Mathf.Max(0, (int)oldCoords.Value.y - 1);

			for (int i = 0; i < 3; ++i)
			{
				for (int j = 0; j < 3; ++j)
				{
					texture.SetPixel(oldXTopLeft + i, oldYTopLeft + j, this.baseColorMap[oldXTopLeft + i, oldYTopLeft + j]);
				}
			}
		}
					
									 
		texture.SetPixel(newX, newY, Color.yellow, 1);
		texture.Apply();        
	}        
}
