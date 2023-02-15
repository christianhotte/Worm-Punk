using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

// Script was used from https://youtu.be/KHWuTBmT1oI?t=1186

public class NetworkPlayerSpawn : MonoBehaviourPunCallbacks
{
    //Objects & Components:
    public static NetworkPlayerSpawn instance; //Singleton instance of this script in scene
    
    private NetworkPlayer clientNetworkPlayer; //Instance of local client's network player in scene

    private string mainMenuScene;

    //Settings:
    [Header("Resource References:")]
    [SerializeField, Tooltip("Exact name of network player prefab in Resources folder.")] private string networkPlayerName;

    private void Awake()
    {
        //Initialization:
        if (instance == null) { instance = this; } else { Debug.LogError("Tried to load two NetworkPlayerSpawn scripts in the same scene!"); Destroy(this); } //Singleton-ize this script
        mainMenuScene = "MainMenu";
    }

    // When someone joins a room, we spawn the player.
    public override void OnJoinedRoom()
    {
        //Initialization:
        base.OnJoinedRoom();

        // The network players should never spawn in the main menu
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name == mainMenuScene)
        {
            return;
        }

        // Spawns network players when you join a room on any other scene besides the main menu.
        else
        {
            //Spawn network player:
            clientNetworkPlayer = PhotonNetwork.Instantiate(networkPlayerName, Vector3.zero, Quaternion.identity).GetComponent<NetworkPlayer>(); //Spawn instance of network player and get reference to its script
            if (clientNetworkPlayer == null) Debug.LogError("Tried to spawn network player prefab that doesn't have NetworkPlayer component!");  //Indicate problem if relevant
        }
    }

    // When someone leaves a room, we want to remove the player from the game.
    public override void OnLeftRoom()
    {
        Debug.Log("A player has left the room.");
        base.OnLeftRoom();
        if (clientNetworkPlayer != null) PhotonNetwork.Destroy(clientNetworkPlayer.gameObject);
    }
}