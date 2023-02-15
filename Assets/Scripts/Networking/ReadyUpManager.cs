using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class ReadyUpManager : MonoBehaviourPunCallbacks
{
    // Can probably have a button that lights up green or red to show if a player is ready through the network.
    //[SerializeField] private GameObject readyButton;

    private int numberOfPlayers; // The number of players in the current room
    [SerializeField] private int playersNeededToStart; // The number of players needed to start the game
    [SerializeField] private string sceneToLoad = "DMars_New_Area";
    private int playersReady = 0; // The number of players that have readied up

    // Start is called before the first frame update
    private void Start()
    {
        playersNeededToStart = PhotonNetwork.CurrentRoom.PlayerCount;
        Debug.Log("Initial Amount of Players Needed To Start: " + playersNeededToStart);
    }

    // Once the room is joined.
    public override void OnJoinedRoom()
    {
        //numberOfPlayers = PhotonNetwork.CurrentRoom.PlayerCount;
        playersNeededToStart++;
    }

    public override void OnLeftRoom()
    {
        playersNeededToStart--;
    }

    // Once the level is pulled to signify that the player is ready...
    public void ReadyLeverPulled()
    {
        photonView.RPC("RPC_UpdateReadyStatus", RpcTarget.AllBuffered);
    }

    // Tells the master server the amount of players that are ready to start the match.
    [PunRPC]
    private void RPC_UpdateReadyStatus()
    {
        // Increase the number of players that have readied up
        playersReady++;

        Debug.Log(playersReady.ToString() + "/" + PhotonNetwork.PlayerList.Length.ToString() + " players ready.");

        // If all players are ready, load the game scene
        if (playersReady == playersNeededToStart && PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(sceneToLoad);
        }
    }
}