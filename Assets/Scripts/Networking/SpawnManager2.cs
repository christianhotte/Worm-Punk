using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnManager2 : MonoBehaviour
{
    public Transform[] spawnPoints; // An array of spawn points for the players

    private int nextSpawnPointIndex = 0; // Index of the next spawn point to use

    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
        {
            // Spawn the players when the game starts
            SpawnPlayer();
        }
    }

    private void SpawnPlayer()
    {
        // Get the position and rotation of the next spawn point
        Transform spawnPoint = spawnPoints[nextSpawnPointIndex];
        Vector3 spawnPosition = spawnPoint.position;
        Quaternion spawnRotation = spawnPoint.rotation;

        // Spawn the Network Player at the spawn point
        //PhotonNetwork.Instantiate("NetworkPlayer", spawnPosition, spawnRotation);

        // Move the demo player to the spawn point
        GameObject demoPlayer = GameObject.Find("DemoPlayer4");
        demoPlayer.transform.position = spawnPosition;
        demoPlayer.transform.rotation = spawnRotation;

        // Update the next spawn point index
        nextSpawnPointIndex = (nextSpawnPointIndex + 1) % spawnPoints.Length;
    }
}