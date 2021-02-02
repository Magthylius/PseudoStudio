using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{ 
    public static SpawnManager instance;

    Spawnpoint[] spawnPoints;

    private void Awake()
    {
        instance = this;
        spawnPoints = GetComponentsInChildren<Spawnpoint>();
    }

    /// <summary>
    /// Give player random spawn point to spawn with
    /// </summary>
    /// <returns></returns>
    public Transform GetSpawnPoint()
    {
        return spawnPoints[Random.Range(0, spawnPoints.Length)].transform;
    }

}
