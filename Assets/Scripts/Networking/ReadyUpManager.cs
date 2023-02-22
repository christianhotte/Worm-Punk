using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using TMPro;

public class ReadyUpManager : MonoBehaviourPunCallbacks
{
    // Can probably have a button that lights up green or red to show if a player is ready through the network.
    //[SerializeField] private GameObject readyButton;

    [SerializeField] private TextMeshProUGUI playerReadyText;

    private const int MINIMUM_PLAYERS_NEEDED = 2;   // The minimum number of players needed for a round to start
    [SerializeField] private string sceneToLoad = "DM_0.11_Arena";

    private int playersReady, playersInRoom;

    private LeverController[] allLevers;
    
    // Is called upon the first frame.
    private void Start()
    {
        allLevers = FindObjectsOfType<LeverController>();
        UpdateReadyText();
    }

    // Once the room is joined.
    public override void OnJoinedRoom()
    {
        UpdateReadyText();

        // If the amount of players in the room is maxed out, close the room so no more people are able to join.
/*        if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
        }*/
    }

    // When a player leaves the room
    public override void OnLeftRoom()
    {
        if(NetworkManagerScript.instance.GetMostRecentRoom() != null)
        {
            if (NetworkManagerScript.instance.GetMostRecentRoom().PlayerCount > 0)
            {
                playersInRoom = NetworkManagerScript.instance.GetMostRecentRoom().PlayerCount;
                UpdateReadyText();
            }
        }

        // The room becomes open to let more people come in.
/*        if (PhotonNetwork.CurrentRoom.PlayerCount < PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            if (PhotonNetwork.InRoom) PhotonNetwork.CurrentRoom.IsOpen = true;
        }*/
    }

    // Once the level is pulled to signify that the player is ready...
    public void ReadyLeverPulled(float leverValue)
    {
        if(leverValue == 1)
        {
            //The player is ready
            NetworkManagerScript.localNetworkPlayer.GetNetworkPlayerStats().isReady = true;
        }

        else
        {
            //The player is not ready
            NetworkManagerScript.localNetworkPlayer.GetNetworkPlayerStats().isReady = false;
        }

        NetworkManagerScript.localNetworkPlayer.SyncStats();

        PlayerController.photonView.RPC("RPC_UpdateReadyStatus", RpcTarget.AllBuffered);
    }

    // Tells the master server the amount of players that are ready to start the match.
    [PunRPC]
    private void RPC_UpdateReadyStatus()
    {
        // Get the number of players that have readied up
        playersReady = GetAllPlayersReady();
        playersInRoom = PhotonNetwork.CurrentRoom.PlayerCount;

        UpdateReadyText();

        // If all players are ready, load the game scene
        if (playersReady == playersInRoom && playersInRoom >= MINIMUM_PLAYERS_NEEDED && PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.LoadLevel(sceneToLoad);
        }
    }

    /// <summary>
    /// Updates the text in the center of the room.
    /// </summary>
    private void UpdateReadyText()
    {
        string message = "Players Ready: " + playersReady.ToString() + "/" + playersInRoom;

        if (playersInRoom < MINIMUM_PLAYERS_NEEDED)
        {
            message += "\n<size=26>Not Enough Players To Start.</size>";
        }

        Debug.Log(message);
        playerReadyText.text = message; // Display the message in the scene
    }

    /// <summary>
    /// Check all of the lever values to see if everyone is ready.
    /// </summary>
    private int GetAllPlayersReady()
    {
        int playersReady = 0;

        // Gets the amount of players that have a readied lever at lowest state.
        foreach(var players in FindObjectsOfType<NetworkPlayer>())
        {
            if (players.GetNetworkPlayerStats().isReady)
                playersReady++;
        }

        return playersReady;
    }
}