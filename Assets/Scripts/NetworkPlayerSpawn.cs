using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

// Script was used from https://youtu.be/KHWuTBmT1oI?t=1186

public class NetworkPlayerSpawn : MonoBehaviourPunCallbacks
{
    private GameObject spawnedPlayerPrefab;

    // When someone joins a room, we spawn the player.
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        spawnedPlayerPrefab = PhotonNetwork.Instantiate("Network Player", transform.position, transform.rotation);
    }

    // When someone leaves a room, we want to remove the player from the game.
    public override void OnLeftRoom()
    {
        Debug.Log("A player has left the room.");
        base.OnLeftRoom();
        PhotonNetwork.Destroy(spawnedPlayerPrefab);
    }
}