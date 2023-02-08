using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro; // Can probably delete later when we switch to physical buttons instead of UI

// Code was referenced from https://www.youtube.com/watch?v=KHWuTBmT1oI

/* MonoBehaviourPunCallbacks allows us to override some of the initial functions that are
being called when we are connected to the server, or someone joins the server/room/etc. */

public class NetworkManagerScript : MonoBehaviourPunCallbacks
{
    [SerializeField] TMP_InputField roomNameInPutField;

    // Start is called before the first frame update
    void Start()
    {
        //We want to connect to the Unity server at the beginning of the game.
        ConnectAndGiveDavidYourIPAddress();
    }

    // This function connects us to the server.
    void ConnectToServer()
    {
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("Trying To Connect To Server...");
    }

    public void ConnectAndGiveDavidYourIPAddress()
    {
        ConnectToServer();
    }

    // If someone tries to create a room while we haven't connected to the master server, it will create an error.

    // When you're connected to the server.
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected To Server.");
        base.OnConnectedToMaster();

        // Joins the lobby
        PhotonNetwork.JoinLobby();
    }

    // Once a player has connected to a lobby.
    public override void OnJoinedLobby()
    {
        //LobbyUIScript.instance.OpenMenu("title");
        Debug.Log("Joined a lobby.");
        base.OnJoinedLobby();

        // Setting up the room options
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsVisible = true; // The player is able to see the room
        roomOptions.IsOpen = true; // The room is open.
        PhotonNetwork.JoinOrCreateRoom("Room 1", roomOptions, TypedLobby.Default);
    }

    // The connection of the room.
    public override void OnJoinedRoom()
    {
        Debug.Log("Joined A Room.");
    }

    // To let us know if/when another player joins the room.
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        Debug.Log("A new player has joined the room.");
    }

    public void CreateRoom()
    {

    }
}