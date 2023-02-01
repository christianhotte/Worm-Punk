using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField, Tooltip("The loading screen GameObject.")] private GameObject loadingScreen;

    private List<AsyncOperation> scenesLoading = new List<AsyncOperation>();

    private void Awake()
    {
        Instance = this;
        //Start with a title screen
        SceneManager.LoadSceneAsync((int)SceneIndexes.TITLESCREEN, LoadSceneMode.Additive);
    }

    /// <summary>
    /// Loads the new scene when starting the game.
    /// </summary>
    /// <param name="sceneIndex"></param>
    public void LoadGame(SceneIndexes sceneIndex)
    {
        //Show the loading screen and add the processes to a list of processes
        loadingScreen.SetActive(true);
        scenesLoading.Add(SceneManager.UnloadSceneAsync((int)SceneIndexes.TITLESCREEN));
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

        //Hide the loading screen when finished
        loadingScreen.SetActive(false);
    }
}
