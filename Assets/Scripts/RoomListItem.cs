using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using TMPro;

// Code was used from https://www.youtube.com/watch?v=KGzMxalSqQE&list=PLhsVv9Uw1WzjI8fEBjBQpTyXNZ6Yp1ZLw&index=2

public class RoomListItem : MonoBehaviour
{
    [SerializeField] TMP_Text text;
    RoomInfo info;

    // Sets the text to the name of the room
    public void SetUp(RoomInfo _info)
    {
        info = _info;
        text.text = _info.Name;
    }

    // When the button is pressed
    public void OnClick()
    {
        // Joins the room that was selected
        NetworkManagerScript.instance.JoinRoom(info);
    }
}