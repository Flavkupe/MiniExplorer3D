using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class ResourceManager
{
    private static Dictionary<string, Room> roomPrefabsByID = new Dictionary<string, Room>();

    public static Area GetEmptyAreaPrefab()
    {
        return Resources.Load<Area>("Prefabs/Area");
    }

    public static List<Room> GetAllRoomPrefabs(AreaTheme theme)
    {
        if (theme == AreaTheme.None)
        {
            return Resources.LoadAll<Room>("Prefabs/Rooms").ToList();
        }
        else
        {
            return Resources.LoadAll<Room>("Prefabs/Rooms/" + theme.ToString()).ToList();
        }
    }    

    public static Room GetRoomByPrefabID(AreaTheme theme, string prefabID)
    {
        Room room = null;
        if (!roomPrefabsByID.ContainsKey(prefabID))
        {
            var path = "Prefabs/Rooms/" + theme.ToString() + "/" + prefabID;
            room = Resources.Load<Room>(path);
            if (room == null)
            {
                Debug.LogError($"Room prefab with ID '{prefabID}' not found in theme '{theme}'.");
                return null;
            }

            roomPrefabsByID[prefabID] = room;
        }
        else 
        {
            room = roomPrefabsByID.GetValueOrDefault(prefabID);
        }

        System.Diagnostics.Debug.Assert(room != null);
        return room;
    }

    public static FloatyText GetFloatyText()
    {
        return Resources.Load<FloatyText>("Prefabs/Text/FloatyText");
    }

    public static GameObject GetSubrenderer()
    {
        return Resources.Load<GameObject>("Prefabs/Misc/Subrenderer");
    }

    public static Enemy GetRandomEnemyOfType(EnemyType types)
    {
        List<Enemy> list = Resources.LoadAll<Enemy>("Prefabs/Characters/Enemies").ToList();
        if (types == 0)
        {
            return list.GetRandom();
        }

        return list.Where(a => (a.Type & types) != 0).GetRandom();        
    }
}
