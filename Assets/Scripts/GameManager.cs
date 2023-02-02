using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private List<AsyncOperation> scenesLoading = new List<AsyncOperation>();

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
        //Add the processes to a list of processes
        scenesLoading.Add(SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex));
        scenesLoading.Add(SceneManager.LoadSceneAsync((int)sceneIndex, LoadSceneMode.Additive));

        StartCoroutine(GetSceneLoadProgress());
    }

    /// <summary>
    /// Keeps track of the scene's loading progress.
    /// </summary>
    /// <returns></returns>
    public IEnumerator GetSceneLoadProgress()
    {
        //Loop while all scenes are loading
        for(int i = 0; i < scenesLoading.Count; i++)
        {
            while (!scenesLoading[i].isDone)
            {
                yield return null;
            }
        }
    }
}
