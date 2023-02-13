using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using Photon.Pun;
using Photon.Realtime;

// This script was used from https://youtu.be/KHWuTBmT1oI?t=1511

public class NetworkPlayer : MonoBehaviour
{
    //Objects & Components:
    internal PlayerController player; //Client playerController associated with this network player
    private GameObject XROrigin;
    internal PhotonView photonView;

    private Transform headRig;
    private Transform leftHandRig;
    private Transform rightHandRig;

    // Declaring the player's VR movements
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;

    // Gets a list of all of the players on the network
    Player[] allPlayers;
    int myNumberInRoom;

    //Player Data
    private PlayerSetup playerSetup;    //The player's setup component

    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();    //Get photonView component from NetworkPlayer object

        // Gets the network player to move with the player instead of just moving locally.
        XROrigin = GameObject.Find("XR Origin");
        player = XROrigin.GetComponentInParent<PlayerController>();
        
        playerSetup = player.GetComponent<PlayerSetup>();
        headRig = XROrigin.transform.Find("Camera Offset/Main Camera");
        leftHandRig = XROrigin.transform.Find("Camera Offset/LeftHand Controller");
        rightHandRig = XROrigin.transform.Find("Camera Offset/RightHand Controller");

        if (photonView.IsMine)
        {
            PlayerController.photonView = photonView; //Give playerController a reference to local client photon view component
            LocalPlayerSettings(PlayerSettings.Instance.charData, false);
            SyncData(PlayerSettings.Instance);
        }

        // Gets the player list
        allPlayers = PhotonNetwork.PlayerList;
        foreach (Player p in allPlayers)
        {
            // Adds more numbers until the number of players match
            if (p != PhotonNetwork.LocalPlayer)
            {
                myNumberInRoom++;
            }
        }

        //Ignore collisions:
        foreach (Collider collider in GetComponentsInChildren<Collider>())
        {
            foreach (Collider otherCollider in XROrigin.transform.parent.GetComponentsInChildren<Collider>())
            {
                Physics.IgnoreCollision(collider, otherCollider);
            }
        }

        //Hide client renderers:
        if (photonView.IsMine)
        {
            foreach (var item in GetComponentsInChildren<Renderer>())
            {
                item.enabled = false;
            }
        }
    }

    private void SyncData(PlayerSettings playerData)
    {
        Debug.Log("Syncing Player Data...");
        string characterData = playerData.CharDataToString();
        photonView.RPC("LoadPlayerSettings", RpcTarget.OthersBuffered, characterData);
    }

    public void LoadPlayerSettings(string data)
    {
        Debug.Log("Loading Player Settings...");
        LocalPlayerSettings(JsonUtility.FromJson<CharacterData>(data), true);
    }

    private void LocalPlayerSettings(CharacterData charData, bool isOnNetwork)
    {
        Debug.Log("Is On Network: " + isOnNetwork);

        Debug.Log("Changing " + charData.playerName + " to " + charData.testColor);
        playerSetup.SetColor(charData.testColor);
    }

    /// <summary>
    /// Indicates that this player has been hit by a networked projectile.
    /// </summary>
    /// <param name="damage">How much damage the projectile dealt.</param>
    [PunRPC]
    public void RPC_Hit(int damage)
    {
        if (photonView.IsMine) player.IsHit(damage); //Inflict damage upon hit player
    }

    private void OnDestroy()
    {
        if (photonView.IsMine) PlayerController.photonView = null; //Clear client photonView referenc
    }

    // Update is called once per frame
    void Update()
    {
        // Synchronizes the player over the network.
        if (photonView.IsMine)
        {
            // Calls these functions to map the position of the player's hands & headset
            MapPosition(head, headRig);
            MapPosition(leftHand, leftHandRig);
            MapPosition(rightHand, rightHandRig);
        }

        // The player dies if the player falls too far below the map.
        if (transform.position.y < -15f)
        {
            // You die/lose ();
        }
    }

    // This synchronizes the positions of the headset & the hand controllers 
    void MapPosition(Transform target, Transform rigTransform)
    {
        target.position = rigTransform.position;
        target.rotation = rigTransform.rotation;
    }
}