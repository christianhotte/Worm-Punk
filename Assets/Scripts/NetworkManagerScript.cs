using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro; // Can probably delete later when we switch to physical buttons instead of UI

/* Code was referenced from https://www.youtube.com/watch?v=KHWuTBmT1oI
 * https://www.youtube.com/watch?v=zPZK7C5_BQo&list=PLhsVv9Uw1WzjI8fEBjBQpTyXNZ6Yp1ZLw */

/* MonoBehaviourPunCallbacks allows us to override some of the initial functions that are
being called when we are connected to the server, or someone joins the server/room/etc. */

public class NetworkManagerScript : MonoBehaviourPunCallbacks
{
    public static NetworkManagerScript instance;

    [SerializeField] TMP_InputField roomNameInPutField;
    [SerializeField] TMP_Text roomNameText;
    [SerializeField] TMP_Text errorText;
    [SerializeField] Transform roomListContent;
    [SerializeField] GameObject roomListItemPrefab;

    // On awake function
    private void Awake()
    {
        // Creates a static reference meaning the variable is bound to the class and not the actual object in Unity; references this script.
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        //We want to connect to the Unity server at the beginning of the game.
        ConnectAndGiveDavidYourIPAddress();
    }

    public void ConnectAndGiveDavidYourIPAddress()
    {
        ConnectToServer();
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
        LobbyUIScript.instance.OpenMenu("title");
        Debug.Log("Joined a lobby.");
        base.OnJoinedLobby();

        // Setting up the room options
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsVisible = true; // The player is able to see the room
        roomOptions.IsOpen = true; // The room is open.
        PhotonNetwork.JoinOrCreateRoom("Room 1", roomOptions, TypedLobby.Default);
    }

    public void CreateRoom()
    {
        // Doesn't allow an empty room name.
        if (string.IsNullOrEmpty(roomNameInPutField.text))
        {
            return;
        }

        // Creates a room with the name of what the player has typed in.
        PhotonNetwork.CreateRoom(roomNameInPutField.text);
        LobbyUIScript.instance.OpenMenu("loading"); // Opens the loading screen while the room is being created on the server.
    }

    // The connection of the room.
    public override void OnJoinedRoom()
    {
        Debug.Log("Joined A Room.");
        // Opens the room menu UI
        LobbyUIScript.instance.OpenMenu("room");
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;
    }

    // We failed to create a room and we will display the error message to the player.
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        //base.OnCreateRoomFailed(returnCode, message);
        errorText.text = "Room Creation Failed: " + message;
        LobbyUIScript.instance.OpenMenu("error");
    }

    // To let us know if/when another player joins the room.
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        Debug.Log("A new player has joined the room.");
    }

    // Leaves the room that a player has entered.
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    // once the player has left the room.
    public override void OnLeftRoom()
    {
        LobbyUIScript.instance.OpenMenu("title");
    }

    // Joins the room that a player selects 
    public void JoinRoom(RoomInfo info)
    {
        // Updates the room text and opens the loading menu.
        PhotonNetwork.JoinRoom(info.Name);
        LobbyUIScript.instance.OpenMenu("loading");
    }

    // Shows us the list of room info
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        //base.OnRoomListUpdate(roomList);

        // We clear the list every time we update.
        foreach (Transform trans in roomListContent)
        {
            Destroy(trans.gameObject);
        }

        // Loops through the list of rooms.
        for (int i = 0; i < roomList.Count; i++)
        {
            // Adds the rooms to the list of rooms.
            Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>().SetUp(roomList[i]);
        }
    }
}