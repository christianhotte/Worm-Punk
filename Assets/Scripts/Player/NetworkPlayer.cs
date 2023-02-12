using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using Photon.Pun;
using Photon.Realtime;
using RootMotion.FinalIK;

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
    private Transform modelBaseRig;

    // Declaring the player's VR movements
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;
    public Transform modelBase;

    // Gets a list of all of the players on the network
    Player[] allPlayers;
    int myNumberInRoom;

    //Settings:
    [Header("Debug Settings:")]
    [SerializeField, Tooltip("Prevents local networkPlayer instance from being hidden on client.")] private bool debugShowLocal;

    //Player Data
    private PlayerSetup playerSetup;    //The player's setup component
    [SerializeField] private CharacterData charData;

    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>(); //Get photonView component from NetworkPlayer object

        // Gets the network player to move with the player instead of just moving locally.
        if (photonView.IsMine)
        {
            XROrigin = GameObject.Find("XR Origin");
            player = XROrigin.GetComponentInParent<PlayerController>();
            playerSetup = player.GetComponent<PlayerSetup>();
            headRig = XROrigin.transform.Find("Camera Offset/Main Camera");
            leftHandRig = XROrigin.transform.Find("Camera Offset/LeftHand Controller");
            rightHandRig = XROrigin.transform.Find("Camera Offset/RightHand Controller");
            modelBaseRig = XROrigin.transform.Find("PlayerModel");
        }
        

        /*if (photonView.IsMine)
        {
            PlayerController.photonView = photonView; //Give playerController a reference to local client photon view component
            playerSetup.SetColor(PlayerSettings.Instance.charData.testColor);
            SyncData(PlayerSettings.Instance);
        }*/

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

        if (photonView.IsMine)
        {
            //Ignore collisions:
            foreach (Collider collider in GetComponentsInChildren<Collider>())
            {
                foreach (Collider otherCollider in XROrigin.transform.parent.GetComponentsInChildren<Collider>())
                {
                    Physics.IgnoreCollision(collider, otherCollider);
                }
            }

            //Hide client renderers:
            if (!debugShowLocal)
            {
                foreach (var item in GetComponentsInChildren<Renderer>())
                {
                    item.enabled = false;
                }
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
        charData = JsonUtility.FromJson<CharacterData>(data);
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
            MapPosition(transform, XROrigin.transform);
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