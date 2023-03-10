using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using TMPro;

public class Leaderboards : MonoBehaviourPunCallbacks
{
    //Objects & Components:
    [Header("Components:")]
    [SerializeField, Tooltip("Displays which rank each player placed in.")]              private TMP_Text ranks;
    [SerializeField, Tooltip("Displays name of each player who participated in match.")] private TMP_Text names;
    [SerializeField, Tooltip("Displays how many kills each player got.")]                private TMP_Text kills;
    [SerializeField, Tooltip("Displays how many time player died")]                      private TMP_Text deaths;

    //Settings:
    [Header("Settings:")]
    [SerializeField, Tooltip("How far apart each new line is (in Y units).")]                                     private float lineSeparation;
    [Range(0, 1), SerializeField, Tooltip("How light player colors are (increase for consistency/readability).")] private float playerColorGamma;
    [SerializeField, Tooltip("If true, system will reset player stats as they leave the scene.")]                 private bool clearStats = true;

    //Runtime Vars:
    private bool showingLeaderboard; //True when leaderboard is enabled for the scene (only when players are coming back from combat)

    //RUNTIME METHODS:
    private void Awake()
    {
        //Event subscription:
        SceneManager.sceneUnloaded += OnSceneUnloaded; //Subscribe to scene unload method
    }
    private void OnDestroy()
    {
        //Event unsubscription:
        SceneManager.sceneUnloaded -= OnSceneUnloaded; //Unsubscribe from scene unload method
    }
    void Start()
    {
        //Initialization:
        if (PhotonNetwork.LocalPlayer.ActorNumber != GetComponentInParent<LockerTubeController>().tubeNumber) { gameObject.SetActive(false); return; } //Hide board if it does not correspond with player's tube

        //Check scene state:
        foreach (NetworkPlayer player in NetworkPlayer.instances) { if (player.networkPlayerStats.numOfKills > 0) { showingLeaderboard = true; break; } } //Show leaderboard if any players have any kills
        if (showingLeaderboard) //Leaderboard is being shown this scene
        {
            //Rank players by K/D:
            List<NetworkPlayer> rankedPlayers = new List<NetworkPlayer>(); //Create a new list for sorting network players by rank
            foreach (NetworkPlayer player in NetworkPlayer.instances) //Iterate through each network player in scene
            {
                if (rankedPlayers.Count == 0) { rankedPlayers.Add(player); continue; } //Add first player immediately to list

                //Get stats:
                PlayerStats stats = player.networkPlayerStats;          //Get current player's stats from last round
                float currentKD = stats.numOfKills / stats.numOfDeaths; //Get KD of current player

                //Rank against competitors:
                for (int x = 0; x < rankedPlayers.Count; x++) //Iterate through ranked player list
                {
                    PlayerStats otherStats = rankedPlayers[x].networkPlayerStats;        //Get stats from other player
                    float otherKD = otherStats.numOfKills / otherStats.numOfDeaths;      //Get KD of other player
                    if (otherKD < currentKD) { rankedPlayers.Insert(x, player); break; } //Insert current player above the first player it outranks
                }
                if (!rankedPlayers.Contains(player)) rankedPlayers.Add(player); //Add player in last if it doesn't outrank anyone
            }

            //Display lists:
            for (int x = 0; x < rankedPlayers.Count; x++) //Iterate through list of ranked players
            {
                //Initialization:
                float yHeight = x * -lineSeparation;                                  //Get target Y height for all new text assets
                PlayerStats stats = rankedPlayers[x].networkPlayerStats;              //Get stats from current player
                Color playerColor = rankedPlayers[x].currentColor;                    //Get color to make text (from synched player settings)
                playerColor = Color.Lerp(playerColor, Color.white, playerColorGamma); //Make color a bit lighter so it is more readable

                //Place rank number:
                TMP_Text newRank = Instantiate(ranks, ranks.transform.parent).GetComponent<TMP_Text>(); //Instantiate new text object
                Vector3 newPos = ranks.transform.localPosition;                                         //Reference position of original text object for new text
                newPos.y = yHeight;                                                                     //Set Y height of text object
                newRank.transform.position = newPos;                                                    //Move text object to new position
                newRank.text = "#" + (x + 1) + ":";                                                     //Display ranks in order by number
                newRank.color = playerColor;                                                            //Set text color to given player color

                //Place name:
                TMP_Text newName = Instantiate(names, names.transform.parent).GetComponent<TMP_Text>(); //Instantiate new text object
                newPos = names.transform.localPosition;                                                 //Reference position of original text object for new text
                newPos.y = yHeight;                                                                     //Set Y height of text object
                newName.transform.position = newPos;                                                    //Move text object to new position
                newName.text = rankedPlayers[x].GetName();                                              //Display player name
                newName.color = playerColor;                                                            //Set text color to given player color

                //Place kills:
                TMP_Text newKills = Instantiate(kills, kills.transform.parent).GetComponent<TMP_Text>(); //Instantiate new text object
                newPos = kills.transform.localPosition;                                                  //Reference position of original text object for new text
                newPos.y = yHeight;                                                                      //Set Y height of text object
                newKills.transform.position = newPos;                                                    //Move text object to new position
                newKills.text = stats.numOfKills.ToString();                                             //Display killcount
                newKills.color = playerColor;                                                            //Set text color to given player color

                //Place kills:
                TMP_Text newDeaths = Instantiate(deaths, deaths.transform.parent).GetComponent<TMP_Text>(); //Instantiate new text object
                newPos = kills.transform.localPosition;                                                     //Reference position of original text object for new text
                newPos.y = yHeight;                                                                         //Set Y height of text object
                newDeaths.transform.position = newPos;                                                      //Move text object to new position
                newDeaths.text = stats.numOfDeaths.ToString();                                              //Display death count
                newDeaths.color = playerColor;                                                              //Set text color to given player color
            }

            //Clear list references:
            ranks.enabled = false;  //Hide original ranking display
            names.enabled = false;  //Hide original name display
            kills.enabled = false;  //Hide original kill display
            deaths.enabled = false; //Hide original death display
        }
        else gameObject.SetActive(false); // If the player did not come back from a game, we don't want to show the leaderboards.
    }
    /// <summary>
    /// Called whenever current scene is unloaded.
    /// </summary>
    public void OnSceneUnloaded(Scene scene)
    {
        if (clearStats) //Leaderboard is set to clear local player stats
        {
            NetworkPlayer localPlayer = PlayerController.photonView.GetComponent<NetworkPlayer>(); //Get reference to local network player
            localPlayer.networkPlayerStats.numOfKills = 0;                                         //Reset kill counter
            localPlayer.networkPlayerStats.numOfDeaths = 0;                                        //Reset death counter
            localPlayer.SyncStats();                                                               //Sync cleared stats over network
        }
    }
}