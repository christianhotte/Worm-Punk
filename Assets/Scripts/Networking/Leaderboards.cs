using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using TMPro;
using System.Linq;

public class Leaderboards : MonoBehaviourPunCallbacks
{
    [SerializeField] TMP_Text leaderboardText;
    private string playerScores;
    private string arenaScene;

    // Start is called before the first frame update
    void Start()
    {
        playerScores = "";
        arenaScene = "Assets/Scenes/DMars_Scenes/DM_0.12_Arena.unity";

        // Gets the name of the last scene we were on (returning from the arena to the network locker room).
        GameManager gameManager = GameManager.Instance;
        string lastSceneName = gameManager.GetLastSceneName();

        Debug.Log("Last Scene Name: " + lastSceneName);

        if (lastSceneName == arenaScene)
        {
            OpenLeaderboards();
        }

        else
        {
            gameObject.SetActive(false);
            Debug.Log("God fucking dammit");
        }
    }

    // Displays the leaderboards of the last game to the player in the locker rooms.
    public void OpenLeaderboards()
    {
        UpdateScoreboard();
        gameObject.SetActive(true);
    }

    // Hides the leaderboards
    public void CloseLeaderboards()
    {
        gameObject.SetActive(false);
    }

    // Updates the leadboards to display accurate information.
    public void UpdateScoreboard()
    {
        // For every player present in the locker room, display their scores
        foreach (NetworkPlayer player in NetworkPlayer.instances)
        {
            // Just adds to the strings
            playerScores += player.GetName() + " Kills: " + player.networkPlayerStats.numOfKills.ToString() + player.GetName() + " Deaths: " + player.networkPlayerStats.numOfDeaths.ToString() + "\n";
        }
    }
}