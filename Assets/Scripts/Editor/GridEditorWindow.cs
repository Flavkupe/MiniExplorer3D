using Assets.Scripts.LevelGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;


public class GridEditorWindow : EditorWindow
{
    [MenuItem("Window/Grid Window")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(GridEditorWindow));
    }

    private Area selectedArea = null;

    int step = 8;
    float gridline = 0.5f;
    // Vector2 scrollPosition = Vector2.zero;
    Vector2 mousePos = Vector2.zero;
    Vector2 mouseGridCoords = Vector2.zero;

    void OnGUI()
    {
        this.wantsMouseMove = true;
        int currentStep = EditorGUI.IntField(new Rect(4, 4, 250, 20), "Step", step);
        float currentGridline = EditorGUI.FloatField(new Rect(4, 28, 250, 20), "Grid", gridline);        

        int topOfGrid = 100;

        if (this.selectedArea == null)
        {
            Area area = GameObject.FindObjectOfType<Area>() as Area;
            if (area != null)
            {
                this.selectedArea = area;
            }
        }
        else
        {
            //scrollPosition = GUI.BeginScrollView(new Rect(4, bottom, 900, 900), scrollPosition, new Rect(0, 0, 1000, 1000), false, false);
            RoomGrid grid = this.selectedArea.RoomGrid;
            for (int x = 0; x < grid.Dimensions; ++x)
            {
                for (int y = 0; y < grid.Dimensions; ++y)
                {
                    Color color = Color.white;
                    if (grid.HasRoomAt(x, y))
                    {
                        color = Color.red;
                    }

                    float xCoord = 4 + x * step + gridline;
                    float yCoord = topOfGrid + (grid.Dimensions - 1 - y) * step + gridline;
                    float dims = step - (2.0f * gridline);

                    EditorGUI.DrawRect(new Rect(xCoord, yCoord, dims, dims), color);
                }
            }
                       
            //GUI.EndScrollView(true);            

            EditorGUI.LabelField(new Rect(4, 52, 250, 20), "Mouse Pos", mousePos.ToString());
            mousePos = Event.current.mousePosition;

            if (mousePos.x > 4 && mousePos.x < grid.Dimensions * step + 4 &&
                mousePos.y > topOfGrid && mousePos.y < grid.Dimensions * step + topOfGrid + 4)
            {
                int mouseGridX = (int)(mousePos.x - 4) / step;
                int mouseGridY = (int)(mousePos.y - topOfGrid) / step;
                mouseGridY = grid.Dimensions - mouseGridY - 1;
                mouseGridCoords = new Vector2(mouseGridX, mouseGridY);

                EditorGUI.LabelField(new Rect(4, 76, 250, 20), "Mouse Grid Coords", mouseGridCoords.ToString());

                if (Event.current.type == EventType.MouseMove)
                {
                    Repaint();
                }
            }            
        }

        if (GUI.changed)
        {
            this.step = currentStep;
            this.gridline = currentGridline;
        }
    }
}

