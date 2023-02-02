using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

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
        SceneManager.LoadScene((int)sceneIndex);
    }
}
