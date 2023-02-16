using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    internal bool levelTransitionActive = false;

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Loads the new scene when starting the game.
    /// </summary>
    /// <param name="sceneIndex"></param>
    public void LoadGame(SceneIndexes sceneIndex)
    {
        Debug.Log("Loading Scene - " + sceneIndex.ToString());
        //SceneManager.LoadScene((int)sceneIndex);
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel((int)sceneIndex);
        }
        levelTransitionActive = false;
    }

    /// <summary>
    /// Determine whether the player is in a menu depending on the active scene name.
    /// </summary>
    /// <returns>If true, the player is in a menu scene. If false, the player is in a combat scene.</returns>
    public bool InMenu()
    {
        switch (SceneManager.GetActiveScene().name)
        {
            case "MainMenu":
                return true;
            case "NetworkLockerRoom":
                return true;
            default:
                return false;
        }
    }
}