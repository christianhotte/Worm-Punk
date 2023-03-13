using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SpawnManager2 : MonoBehaviourPunCallbacks
{
    [SerializeField] private Transform[] spawnPoints;

    private GameObject demoPlayer;

    // Called on the first frame.
    void Start()
    {
        demoPlayer = PlayerController.instance.gameObject;
        if (demoPlayer == null)
        {
            Debug.LogError("Player not found in scene.");
            return;
        }

        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogError("Photon network is not connected and ready.");
            return;
        }

        MoveDemoPlayerToSpawnPoint();
    }

    // Moves the local demo player to a spawn point.
    private void MoveDemoPlayerToSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("Spawn points not set.");
            return;
        }

        int spawnPointIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        Debug.Log("Actor Number: " + spawnPointIndex);
        Transform spawnPoint = spawnPoints[spawnPointIndex];

        demoPlayer.transform.position = spawnPoint.position;
        //demoPlayer.transform.rotation = spawnPoint.rotation;
    }
}