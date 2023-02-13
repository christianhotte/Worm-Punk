using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using TMPro;

// Code was used from https://www.youtube.com/watch?v=KGzMxalSqQE&list=PLhsVv9Uw1WzjI8fEBjBQpTyXNZ6Yp1ZLw&index=2

public class RoomListItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    private RoomInfo currentRoomInfo;

    // Sets the text to the name of the room
    public void SetUp(RoomInfo roomInfo)
    {
        currentRoomInfo = roomInfo;
        text.text = roomInfo.Name + " - " + roomInfo.PlayerCount + "/" + roomInfo.MaxPlayers;
    }

    // When the button is pressed
    public void OnClick()
    {
        // Joins the room that was selected
        Debug.Log("Joining " + currentRoomInfo.Name + "...");
        NetworkManagerScript.instance.JoinRoom(text.text);
    }

    public RoomInfo GetRoomListInfo() => currentRoomInfo;
}