using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void GoToArena()
    {
/*        if (GameManager.Instance != null)
            GameManager.Instance.LoadGame(SceneIndexes.ARENA);
        else
            SceneManager.LoadScene((int)SceneIndexes.ARENA);*/
    }

    public void ToggleDoor(DoorController doorController)
    {
        //If the door is not open, open the door
        if(!doorController.IsDoorOpen())
            doorController.OpenDoor();
        else
            doorController.CloseDoor();
    }
}
