using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SpawnManager2 : MonoBehaviourPunCallbacks
{
    public Transform[] spawnPoints; // An array of spawn points for the players

    private int nextSpawnPointIndex = 0; // Index of the next spawn point to use

    public override void OnJoinedRoom()
    {
        // Move the demo player to the appropriate spawn point
        MoveDemoPlayerToSpawnPoint(PhotonNetwork.LocalPlayer);
    }

    private void MoveDemoPlayerToSpawnPoint(Player player)
    {
        // Get the demo player for this player's client
        GameObject demoPlayer = player.TagObject as GameObject;
        if (demoPlayer != null)
        {
            // Get the position and rotation of the next spawn point
            Transform spawnPoint = spawnPoints[nextSpawnPointIndex];
            Vector3 spawnPosition = spawnPoint.position;
            //Quaternion spawnRotation = spawnPoint.rotation;

            // Move the demo player to the spawn point
            demoPlayer.transform.position = spawnPosition;
            //demoPlayer.transform.rotation = spawnRotation;

            // Update the next spawn point index
            UpdateSpawnPoint();
        }
    }

    //On left room as well.

    private void UpdateSpawnPoint()
    {
        // Update the next spawn point index
        nextSpawnPointIndex = (nextSpawnPointIndex + 1) % spawnPoints.Length;
    }
}