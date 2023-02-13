using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Code was used from https://youtu.be/VtT6ZEcCVus?t=148

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager instance; // Creates a singleton for the script
    //public Transform[] spawnPoints; // A list of spawn points
    public List<Transform> spawnPoints = new List<Transform>();
    private List<Transform> usedSpawnPoints = new List<Transform>();

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
    }

    public Transform GetSpawnPoint()
    {
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
        usedSpawnPoints.Add(spawnPoint);
        spawnPoints.Remove(spawnPoint);
        
        if (spawnPoints.Count == 0)
        {
            foreach (Transform point in usedSpawnPoints) spawnPoints.Add(point);
            usedSpawnPoints = new List<Transform>();
        }

        return spawnPoint;
    }
}