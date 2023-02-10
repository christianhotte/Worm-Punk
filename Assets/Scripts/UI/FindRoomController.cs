using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindRoomController : MonoBehaviour
{
    [SerializeField] private SliderController slider;
    [SerializeField] private RectTransform scrollArea;
    [SerializeField] private float menuItemHeight = 22.59f;

    private RoomListItem[] listedRooms;

    private void Start()
    {
        listedRooms = GetComponentsInChildren<RoomListItem>();
    }

    public void ChangeScrollAreaPosition(float sliderPos)
    {
        float maximumYPos = menuItemHeight * (listedRooms.Length - 1);

        Vector3 scrollLocalPos = scrollArea.localPosition;
        scrollLocalPos.y = maximumYPos * sliderPos;
        scrollLocalPos.y = Mathf.Clamp(scrollLocalPos.y, 0, maximumYPos);

        scrollArea.anchoredPosition = scrollLocalPos;
    }
}
