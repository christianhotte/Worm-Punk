using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

/* Code was referenced from https://www.youtube.com/watch?v=KHWuTBmT1oI
 * https://www.youtube.com/watch?v=zPZK7C5_BQo&list=PLhsVv9Uw1WzjI8fEBjBQpTyXNZ6Yp1ZLw */

/* MonoBehaviourPunCallbacks allows us to override some of the initial functions that are
being called when we are connected to the server, or someone joins the server/room/etc. */

public class NetworkManagerScript : MonoBehaviourPunCallbacks
{
    //Objects & Components:
    public static NetworkManagerScript instance;    //Singleton-ized instance of this script in scene
    public static NetworkPlayer localNetworkPlayer; //Instance of local client's network player in scene

    //Settings:
    [Tooltip("Turn this on to force player to join room as soon as game is loaded.")] public bool joinRoomOnLoad = false;
    [Tooltip("Name of primary menu scene.")]                                          public string mainMenuScene;
    [Tooltip("Name of primary multiplayer room scene.")]                              public string roomScene;
    [SerializeField, Tooltip("Name of network player prefab in Resources folder.")]   private string networkPlayerName;

    //Runtime Variables:


    //RUNTIME METHODS:
    private void Awake()
    {
        //Initialize:
        if (instance == null) { instance = this; } else Destroy(gameObject); //Singleton-ize this script instance

        //Get objects & components:

    }
    void Start()
    {
        ConnectAndGiveDavidYourIPAddress(); //Immediately start trying to connect to master server
    }

    //NETWORK FUNCTIONS:
    public void ConnectAndGiveDavidYourIPAddress()
    {
        if (!PhotonNetwork.IsConnected) { ConnectToServer(); }
    }
    void ConnectToServer()
    {
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("Trying To Connect To Server...");
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
    public void JoinRoom(string roomName)
    {
        // Joins the room on the network
        PhotonNetwork.JoinRoom(roomName);

        if (PhotonNetwork.InRoom) Debug.Log("Successfully Connected To " + roomName);
    }
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

    //NETWORK CALLBACKS:
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected To Server.");
        base.OnConnectedToMaster();

        // Joins the lobby
        PhotonNetwork.JoinLobby();
    }
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

        GenerateRandomNickname();

        // Setting up the room options
        if (joinRoomOnLoad && !PhotonNetwork.InRoom)
        {
            OnCreateRoom("Dev. Test Room");
        }
    }

    private string[] wormNames = { "Unfortunate Worm", "Sad Intertebrate", "Desolate Earth Creature", "Slimy Specimen", "Grotesque Grub", "Manic Wormling" };

    /// <summary>
    /// Generates a random nickname for the player.
    /// </summary>
    private void GenerateRandomNickname()
    {
        Random.InitState(System.DateTime.Now.Millisecond);  //Seeds the randomizer
        string currentWormName = wormNames[Random.Range(0, wormNames.Length)];
        SetPlayerNickname(currentWormName + " #" + Random.Range(0, 1000).ToString("0000"));
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
    public override void OnJoinedRoom()
    {
        //Update lobby UI:
        LobbyUIScript lobbyUI = FindObjectOfType<LobbyUIScript>();
        if (lobbyUI != null) //If there is a lobby in the scene, display room information
        {
            lobbyUI.UpdateRoomList();
            lobbyUI.OpenMenu("room");
            lobbyUI.ShowLaunchButton(true);
        }

        //Cleanup:
        Debug.Log("Joined " + PhotonNetwork.CurrentRoom.Name + " room."); //Indicate that room has been joined
        SpawnNetworkPlayer();                                             //Always spawn a network player instance when joining a room
    }
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
    public override void OnLeftRoom()
    {
        //Update lobby script:
        LobbyUIScript lobbyUI = FindObjectOfType<LobbyUIScript>();
        if (lobbyUI != null)
        {
            lobbyUI.OpenMenu("title");
        }

        //Cleanup:
        DeSpawnNetworkPlayer(); //De-spawn local network player whenever player leaves a room
    }
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
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Disconnected from server for reason " + cause.ToString());
    }

    //FUNCTIONALITY METHODS:
    public void SpawnNetworkPlayer()
    {
        if (localNetworkPlayer != null) { Debug.LogError("Tried to spawn a second NetworkPlayer for local client."); return; }              //Abort if player already has a network player
        localNetworkPlayer = PhotonNetwork.Instantiate(networkPlayerName, Vector3.zero, Quaternion.identity).GetComponent<NetworkPlayer>(); //Spawn instance of network player and get reference to its script
    }
    public void DeSpawnNetworkPlayer()
    {
        if (localNetworkPlayer != null) PhotonNetwork.Destroy(localNetworkPlayer.gameObject); //Destroy local network player if possible
        localNetworkPlayer = null;                                                            //Remove reference to destroyed reference player
    }
    public void SetPlayerNickname(string name)
    {
        PhotonNetwork.NickName = name;
        PlayerSettings.Instance.charData.playerName = PhotonNetwork.NickName;
    }

    //UTILITY METHODS:
    public List<string> GetPlayerNameList()
    {
        List<string> playerNameList = new List<string>();

        foreach (var player in GetPlayerList())
        {
            playerNameList.Add(player.NickName);
        }

        return playerNameList;
    }
    public string GetCurrentRoom() => PhotonNetwork.CurrentRoom.Name;
    public Player[] GetPlayerList() => PhotonNetwork.PlayerList;
    public string GetLocalPlayerName() => PhotonNetwork.LocalPlayer.NickName;
    public bool IsLocalPlayerInRoom() => PhotonNetwork.InRoom;
}