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
        arenaScene = "DM_0.12_Arena";

        Debug.Log("Last Scene Name: " + GameManager.Instance.prevSceneName);

        // If the player has just returned from the a game after returning from the arena, open up the game leaderboards
        if (GameManager.Instance.prevSceneName == arenaScene)
        {
            OpenLeaderboards();
        }

        // If the player did not come back from a game, we don't want to show the leaderboards.
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
            leaderboardText.text += player.GetName() + " Kills: " + player.networkPlayerStats.numOfKills.ToString() + player.GetName() + " Deaths: " + player.networkPlayerStats.numOfDeaths.ToString() + "\n";
        }
    }
}