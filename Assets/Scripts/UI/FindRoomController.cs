using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FindRoomController : MonoBehaviour
{
    [SerializeField] private SliderController slider;
    [SerializeField] private RectTransform arrowObject;
    [SerializeField] private RectTransform scrollArea;
    [SerializeField] private float menuItemHeight = 22.59f;

    private RoomListItem selectedRoom;

    private RoomListItem[] listedRooms;

    private void OnEnable()
    {
        RefreshRoomListItems();

        if(listedRooms.Length > 0)
        {
            selectedRoom = listedRooms[0];
            selectedRoom.OnSelect();
        }

        UpdateMenu();
    }

    /// <summary>
    /// Changes the scroll area's position.
    /// </summary>
    /// <param name="sliderPos">The position of the slider.</param>
    public void ChangeScrollAreaPosition(float sliderPos)
    {
        float maximumYPos = menuItemHeight * (listedRooms.Length - 1);

        Vector3 scrollLocalPos = scrollArea.localPosition;
        scrollLocalPos.y = maximumYPos * sliderPos;
        scrollLocalPos.y = Mathf.Clamp(scrollLocalPos.y, 0, maximumYPos);

        scrollArea.anchoredPosition = scrollLocalPos;

        UpdateMenu();
    }

    /// <summary>
    /// Updates the menu and selects an active room
    /// </summary>
    private void UpdateMenu()
    {
        Debug.Log("Arrow: " + arrowObject.GetComponent<RectTransform>().position.y);

        float arrowYPos = arrowObject.GetComponent<RectTransform>().position.y;

        int counter = 1;
        foreach (var rooms in listedRooms)
        {
            Debug.Log(" Room Menu Item "+ counter + " Position: " + rooms.GetComponent<RectTransform>().position.y);
            Debug.Log("Arrow and Menu Item Distance: " + Mathf.Abs(arrowYPos - rooms.GetComponent<RectTransform>().position.y));
            if (Mathf.Abs(arrowYPos - rooms.GetComponent<RectTransform>().position.y) < menuItemHeight / 2f)
            {
                Debug.Log("Selecting Room " + counter + "...");
                rooms.OnSelect();
                if (selectedRoom != null)
                    selectedRoom.OnDeselect();
                selectedRoom = rooms;
            }
            counter++;
        }
    }
    
    /// <summary>
    /// Connects to the selected room.
    /// </summary>
    public void ConnectToRoom()
    {
        if(selectedRoom != null)
            selectedRoom.OnClick();
    }

    /// <summary>
    /// Refreshes the list of rooms.
    /// </summary>
    public void RefreshRoomListItems()
    {
        listedRooms = scrollArea.GetComponentsInChildren<RoomListItem>();
        UpdateMenu();
    }
}
