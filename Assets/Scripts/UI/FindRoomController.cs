using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FindRoomController : MonoBehaviour
{
    [SerializeField] private SliderController slider;
    [SerializeField] private RectTransform arrowObject;
    [SerializeField] private RectTransform scrollArea;

    private float menuItemDistance = 22.59f;
    private float menuItemGlobalHeight;
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
        float maximumYPos = menuItemDistance * (listedRooms.Length - 1);

        Debug.Log("Menu Item Distance: " + menuItemDistance);

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
        float arrowYPos = arrowObject.GetComponent<RectTransform>().position.y;

        if (listedRooms.Length > 1)
        {
            menuItemGlobalHeight = Mathf.Abs(listedRooms[1].GetComponent<RectTransform>().position.y - listedRooms[0].GetComponent<RectTransform>().position.y);
        }

        int counter = 1;
        foreach (var rooms in listedRooms)
        {
            if (rooms == null)
                return;

            if (Mathf.Abs(arrowYPos - rooms.GetComponent<RectTransform>().position.y) < menuItemGlobalHeight / 2f)
            {
                if (selectedRoom != null)
                    selectedRoom.OnDeselect();

                selectedRoom = rooms;
                selectedRoom.OnSelect();
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
