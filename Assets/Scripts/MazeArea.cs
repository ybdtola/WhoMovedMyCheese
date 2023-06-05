using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeArea : MonoBehaviour
{
    static Level level_instance;
    static SpawnController spawn_cheese;

    /// <summary>
    /// Initialize level and spawncontroller 
    /// </summary>
    void Start()
    {
        level_instance = GetComponent<Level>();
        spawn_cheese = GetComponent<SpawnController>();
    }

    /// <summary>
    /// Get the position of first maze cell
    /// </summary>
    /// <returns></returns>
    public (float, float) GetAreaBoundsFirst()
    {
        float firstItemPosX = spawn_cheese.GetFirstItemPos(level_instance).min.x;
        float firstItemPosZ = spawn_cheese.GetFirstItemPos(level_instance).min.z;
        return (firstItemPosX, firstItemPosZ);
    }

    /// <summary>
    /// Get the position of last maze cell
    /// </summary>
    /// <returns></returns>
    public (float, float) GetAreaBoundsLast()
    {
        float lastItemPosX = spawn_cheese.GetLastItemPos(level_instance).max.x;
        float lastItemPosZ = spawn_cheese.GetLastItemPos(level_instance).max.z;
        return (lastItemPosX, lastItemPosZ);
    }
}
