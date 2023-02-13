using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using TMPro;

// Code was used from https://youtu.be/zPZK7C5_BQo?t=781

public class LobbyUIScript : MonoBehaviour
{
    [SerializeField] TMP_InputField roomNameInPutField;
    [SerializeField] TMP_Text roomNameText;
    [SerializeField] TMP_Text playerNameText;
    [SerializeField] TMP_Text errorText;
    [SerializeField] Transform roomListContent;
    [SerializeField] Transform playerListContent;
    [SerializeField] GameObject roomListItemPrefab;
    [SerializeField] GameObject playerListItemPrefab;

    [SerializeField] Menus[] menus;

    private List<string> playerList;

    // Awake is called first thing.
    void Awake()
    {
        playerList = new List<string>();
    }

    // Easier to call the  open menu method through script
    public void OpenMenu(string menuName)
    {
        // Loops through all the menus in the Canvas.
        for (int i = 0; i < menus.Length; i++)
        {
            // If the menu matches with the menu name we're trying to open...
            if (menus[i].menuName == menuName)
            {
                // Then we can open the menu
                OpenMenu(menus[i]);

                // Clear the input box if going to create room menu
                if (menuName == "create room")
                    ClearNameBox();
            }

            // If it's not the menu we're trying to open, then we want to close it.
            else if (menus[i].open)
            {
                CloseMenu(menus[i]);
            }
        }
    }

    /// <summary>
    /// Adds letters to the room name box.
    /// </summary>
    /// <param name="segment">The segment of letter(s) to add.</param>
    public void AddLetterToNameBox(string segment)
    {
        roomNameInPutField.text += segment;
    }

    /// <summary>
    /// Clears the name box text.
    /// </summary>
    public void ClearNameBox()
    {
        roomNameInPutField.text = "";
    }

    public void UpdateErrorMessage(string errorMessage)
    {
        errorText.text = errorMessage;
    }

    // Easier to call the  open menu method through hierarchy.
    public void OpenMenu(Menus menu)
    {
        // If the menu is open, we want to close it because we only want one menu open at a time.
        for (int i = 0; i < menus.Length; i++)
        {
            CloseMenu(menus[i]);
        }

        // Opens the menu.
        menu.Open();
    }

    // Closes the menu (easier through hierarchy).
    public void CloseMenu(Menus menu)
    {
        menu.Close();
    }

    public void CreateRoom()
    {
        // Doesn't allow an empty room name.
        if (string.IsNullOrEmpty(roomNameInPutField.text))
        {
            return;
        }

        OpenMenu("loading"); // Opens the loading screen
        // Creates a room with the name of what the player has typed in.
        NetworkManagerScript.instance.OnCreateRoom(roomNameInPutField.text);
    }

    public void JoinRoom(string roomName)
    {
        OpenMenu("loading");
        NetworkManagerScript.instance.JoinRoom(roomName);
    }

    public void LeaveRoom()
    {
        NetworkManagerScript.instance.LeaveRoom();
    }

    public void UpdateLobbyList(List<RoomInfo> roomListInfo)
    {
        // We clear the list every time we update.
        foreach (Transform trans in roomListContent)
        {
            Destroy(trans.gameObject);
        }

        // Loops through the list of rooms.
        for (int i = 0; i < roomListInfo.Count; i++)
        {
            if(roomListInfo[i].PlayerCount > 0)
            {
                // Adds the rooms to the list of rooms.
                Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>().SetUp(roomListInfo[i]);
            }
        }

        //Refresh the find room menu's list of rooms
        FindRoomController findRoomController = FindObjectOfType<FindRoomController>();

        if (findRoomController != null)
            findRoomController.RefreshRoomListItems();
    }

    public void OpenRoomList()
    {
        UpdateRoomList();
        OpenMenu("room");
    }

    public void UpdateRoomList()
    {
        // Opens the room menu UI
        roomNameText.text = NetworkManagerScript.instance.GetCurrentRoom();
        playerNameText.text = NetworkManagerScript.instance.GetLocalPlayerName();

        playerList = NetworkManagerScript.instance.GetPlayerNameList();

        // Loops through the list of players and adds to the list of players in the room.
        for (int i = 0; i < playerList.Count; i++)
        {
            Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(playerList[i]);
        }
    }
}