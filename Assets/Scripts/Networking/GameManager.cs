using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        SceneManager.LoadScene((int)sceneIndex);
    }
}