using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public enum MenuArea { SETTINGS, FINAL }

    private PlayerController playerObject;
    [SerializeField, Tooltip("The positions for where the player moves to in the menu areas.")] private Transform[] menuLocations;

    private void Start()
    {
        playerObject = FindObjectOfType<PlayerController>();
    }

    public void GoToArena()
    {
        /*        if (GameManager.Instance != null)
                    GameManager.Instance.LoadGame(SceneIndexes.ARENA);
                else
                    SceneManager.LoadScene((int)SceneIndexes.ARENA);*/
    }

    /// <summary>
    /// Transports the player to the settings area.
    /// </summary>
    /// <param name="speed">The number of seconds it takes to move from the main area to the settings area.</param>
    public void TransportToSettings(float speed)
    {
        StartCoroutine(MovePlayerInMenu(MenuArea.SETTINGS, speed));
    }

    /// <summary>
    /// Transports the player to the final area.
    /// </summary>
    /// <param name="speed">The number of seconds it takes to move from the main area to the final area.</param>
    public void TransportToFinal(float speed)
    {
        //NetworkManagerScript.instance.JoinLobby();
        StartCoroutine(MovePlayerInMenu(MenuArea.FINAL, speed));
    }

    /// <summary>
    /// Launch the player into the sky.
    /// </summary>
    public void LaunchPlayer()
    {
        if (NetworkManagerScript.instance.IsLocalPlayerInRoom())
        {
            GameManager.Instance.levelTransitionActive = true;
            StartCoroutine(LaunchPlayerSequence());
        }
    }

    private IEnumerator LaunchPlayerSequence()
    {
        //Launch the player with an upward force
        playerObject.GetComponentInChildren<Rigidbody>().AddForce(Vector3.up * 10f, ForceMode.Impulse);

        yield return new WaitForSeconds(0.5f);
        GameManager.Instance.LoadGame(SceneIndexes.NETWORKLOCKERROOM);
    }

    private IEnumerator MovePlayerInMenu(MenuArea menuArea, float speed)
    {
        //Get the starting position and ending position based on the area the player is moving to
        Vector3 startingPos = playerObject.transform.localPosition;
        Vector3 endingPos = menuLocations[(int)menuArea].position;

        //Move the player with a lerp
        float timeElapsed = 0;

        while (timeElapsed < speed)
        {
            //Smooth lerp duration algorithm
            float t = timeElapsed / speed;
            t = t * t * (3f - 2f * t);

            playerObject.transform.localPosition = Vector3.Lerp(startingPos, endingPos, t);    //Lerp the player's movement

            timeElapsed += Time.deltaTime;

            yield return null;
        }
    }
}
