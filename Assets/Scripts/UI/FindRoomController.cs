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
    [SerializeField] private Color selectedRoomColor = new Color(1, 0, 0, 1);

    private Color defaultColor;

    private RoomListItem selectedRoom;

    private RoomListItem[] listedRooms;

    private void Start()
    {
        defaultColor = new Color(1, 1, 1, 1);
    }

    private void OnEnable()
    {
        RefreshRoomListItems();

        if(listedRooms.Length > 0)
        {
            selectedRoom = listedRooms[0];
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
        Debug.Log("Arrow: " + arrowObject.GetComponent<RectTransform>().anchoredPosition.y);

        float arrowYPos = arrowObject.GetComponent<RectTransform>().anchoredPosition.y;

        int counter = 1;
        foreach (var rooms in listedRooms)
        {
            Debug.Log(" Room Menu Item "+ counter + " Position: " + rooms.GetComponent<RectTransform>().anchoredPosition.y);
            Debug.Log("Arrow and Menu Item Distance: " + Mathf.Abs(arrowYPos - rooms.GetComponent<RectTransform>().anchoredPosition.y));
            if (Mathf.Abs(arrowYPos - rooms.GetComponent<RectTransform>().anchoredPosition.y) < menuItemHeight / 2f)
            {
                Debug.Log("Selecting Room " + counter + "...");
                rooms.GetComponent<Image>().color = selectedRoomColor;
                if(selectedRoom != null)
                    selectedRoom.GetComponent<Image>().color = defaultColor;
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
