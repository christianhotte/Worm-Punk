using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

/* Code was referenced from https://www.youtube.com/watch?v=KHWuTBmT1oI
 * https://www.youtube.com/watch?v=zPZK7C5_BQo&list=PLhsVv9Uw1WzjI8fEBjBQpTyXNZ6Yp1ZLw */

/* MonoBehaviourPunCallbacks allows us to override some of the initial functions that are
being called when we are connected to the server, or someone joins the server/room/etc. */

public class NetworkManagerScript : MonoBehaviourPunCallbacks
{
    public static NetworkManagerScript instance;
    
    public bool joinRoomOnLoad = true;

    private GameObject init;

    // On awake function
    private void Awake()
    {
        // Creates a static reference meaning the variable is bound to the class and not the actual object in Unity; references this script.
        if (instance == null)
        {
            instance = this;
        }
        else
            Destroy(gameObject);
        init = FindObjectOfType<GameManager>().gameObject;
    }

    // Start is called before the first frame update
    void Start()
    {
        //We want to connect to the Unity server at the beginning of the game.
        ConnectAndGiveDavidYourIPAddress();
        //PlayerController.instance.transform.SetParent(init.transform);
    }

    public void ConnectAndGiveDavidYourIPAddress()
    {
        if (!PhotonNetwork.IsConnected)
        {
            ConnectToServer();
        }
    }

    // This function connects us to the server.
    void ConnectToServer()
    {
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("Trying To Connect To Server...");
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
        LobbyUIScript lobbyUI = FindObjectOfType<LobbyUIScript>();

        //If there is a lobby in the scene, show the title screen
        if (lobbyUI != null)
        {
            lobbyUI.OpenMenu("title");
        }

        Debug.Log("Joined a lobby.");
        base.OnJoinedLobby();
        SetPlayerNickname("Player " + Random.Range(0, 1000).ToString("0000"));

        // Setting up the room options
        if (joinRoomOnLoad && !PhotonNetwork.InRoom)
        {
            OnCreateRoom("Dev. Test Room");
        }
    }

    public void SetPlayerNickname(string name)
    {
        PhotonNetwork.NickName = name;
        PlayerSettings.Instance.charData.playerName = PhotonNetwork.NickName;
    }
    public void OnCreateRoom(string roomName)
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsVisible = true; // The player is able to see the room
        roomOptions.IsOpen = true; // The room is open.
        roomOptions.EmptyRoomTtl = 0; // Leave the room open for 0 milliseconds after the room is empty
        roomOptions.MaxPlayers = 6;
        PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
    }

    public override void OnCreatedRoom()
    {
        LobbyUIScript lobbyUI = FindObjectOfType<LobbyUIScript>();

        //If there is a lobby in the scene, display room information
        if (lobbyUI != null)
        {
            lobbyUI.OpenMenu("room");
        }
    }

    // The connection of the room [Also spawns a network player in NetworkPlayerSpawn]
    public override void OnJoinedRoom()
    {
        Debug.Log("Joined " + PhotonNetwork.CurrentRoom.Name + " room.");

        LobbyUIScript lobbyUI = FindObjectOfType<LobbyUIScript>();

        //If there is a lobby in the scene, display room information
        if (lobbyUI != null)
        {
            lobbyUI.UpdateRoomList();
            lobbyUI.OpenMenu("room");
            lobbyUI.ShowLaunchButton(true);
        }
    }

    // If the room fails to join
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Join Room Failed. Reason: " + message);

        LobbyUIScript lobbyUI = FindObjectOfType<LobbyUIScript>();

        //If there is a lobby in the scene, display an error message
        if (lobbyUI != null)
        {
            lobbyUI.UpdateErrorMessage("Join Room Failed. Reason: " + message);
            lobbyUI.OpenMenu("error");
        }
    }

    // We failed to create a room and we will display the error message to the player.
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Create Room Failed: " + returnCode);

        //base.OnCreateRoomFailed(returnCode, message);
        string errorMessage = "Room Creation Failed: " + message;

        LobbyUIScript lobbyUI = FindObjectOfType<LobbyUIScript>();

        //If there is a lobby in the scene, display an error message
        if (lobbyUI != null)
        {
            lobbyUI.UpdateErrorMessage(errorMessage);
            lobbyUI.OpenMenu("error");
        }
    }

    // To let us know if/when another player joins the room.
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        Debug.Log("A new player has joined the room.");

        LobbyUIScript lobbyUI = FindObjectOfType<LobbyUIScript>();

        //Update room information
        if (lobbyUI != null)
        {
            lobbyUI.UpdateRoomList();
        }
    }

    // Leaves the room that a player has entered.
    public void LeaveRoom()
    {
        LobbyUIScript lobbyUI = FindObjectOfType<LobbyUIScript>();
        //Update room information
        if (lobbyUI != null)
        {
            lobbyUI.UpdateRoomList();
            lobbyUI.ShowLaunchButton(false);
        }

        PhotonNetwork.LeaveRoom();
    }

    // once the player has left the room.
    public override void OnLeftRoom()
    {
        LobbyUIScript lobbyUI = FindObjectOfType<LobbyUIScript>();

        //If there is a lobby in the scene, go to the title menu
        if (lobbyUI != null)
        {
            lobbyUI.OpenMenu("title");
        }
    }

    // Joins the room that a player selects 
    public void JoinRoom(string roomName)
    {
        // Joins the room on the network
        PhotonNetwork.JoinRoom(roomName);

        if (PhotonNetwork.InRoom)
        {
            Debug.Log("Successfully Connected To " + roomName);
        }
    }

    // Shows us the list of room info
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        //base.OnRoomListUpdate(roomList);

        Debug.Log("Updating Lobby List...");

        LobbyUIScript lobbyUI = FindObjectOfType<LobbyUIScript>();

        //If there is a lobby in the scene, update the room list
        if (lobbyUI != null)
        {
            lobbyUI.UpdateLobbyList(roomList);
        }
    }

    public List<string> GetPlayerNameList()
    {
        List<string> playerNameList = new List<string>();

        foreach (var player in GetPlayerList())
        {
            playerNameList.Add(player.NickName);
        }

        return playerNameList;
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Disconnected from server for reason " + cause.ToString());
    }

    public string GetCurrentRoom() => PhotonNetwork.CurrentRoom.Name;
    public Player[] GetPlayerList() => PhotonNetwork.PlayerList;
    public string GetLocalPlayerName() => PhotonNetwork.LocalPlayer.NickName;
    public bool IsLocalPlayerInRoom() => PhotonNetwork.InRoom;
}