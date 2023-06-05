using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class Level : MonoBehaviour
{

    public GameObject player;
    public GameObject cheese;
    private SpawnController spawnCheese;
    Vector3 playerPos;

    /// <summary>
    /// Initialize player and cheese position at first frame update
    /// </summary>
    void Start()
    {
        playerPos = player.transform.localPosition;
        spawnCheese = GetComponent<SpawnController>();
        Instantiate(player, new Vector3(0f, 1f, 0f), Quaternion.identity);
        spawnCheese.SpawnCheese(cheese);
    }
}
