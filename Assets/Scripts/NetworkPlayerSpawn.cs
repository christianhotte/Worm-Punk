using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

// Script was used from https://youtu.be/KHWuTBmT1oI?t=1186

public class NetworkPlayerSpawn : MonoBehaviourPunCallbacks
{
    public static NetworkPlayerSpawn instance;
    
    private GameObject spawnedPlayerPrefab;
    private GameObject demoPlayer1;

    private void Awake()
    {
        instance = this;
    }
    // When someone joins a room, we spawn the player.
    public override void OnJoinedRoom()
    {
        demoPlayer1 = GameObject.Find("DemoPlayer1");
        // Spawns the network player at a random spawn location when the player joins a room.
        base.OnJoinedRoom();
        Transform spawnpoint = SpawnManager.instance.GetSpawnPoint();
        spawnedPlayerPrefab = PhotonNetwork.Instantiate("Network Player", Vector3.zero, Quaternion.identity);
        demoPlayer1.transform.position = new Vector3(spawnpoint.position.x, spawnpoint.position.y, spawnpoint.position.z);
        demoPlayer1.transform.eulerAngles = new Vector3(spawnpoint.rotation.x, spawnpoint.rotation.y, spawnpoint.rotation.z);
    }

    // When someone leaves a room, we want to remove the player from the game.
    public override void OnLeftRoom()
    {
        Debug.Log("A player has left the room.");
        base.OnLeftRoom();
        PhotonNetwork.Destroy(spawnedPlayerPrefab);
    }
}