using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using CustomEnums;

/// <summary>
/// Governs networked shotgun firing.
/// </summary>
public class RemoteShotgunController : MonoBehaviourPunCallbacks
{
    //Objects & Components:
    internal NewShotgunController clientGun; //Client weapon associated with this networked weapon (NOTE: this reference is only present on the local client version of this script)
    internal string projectileResourceName;  //Name of projectile to be fired in Resources folder
    private AudioSource audioSource;         //Audio source component on this remote shotgun

    //Runtime Variables:
    private Handedness handedness; //Which hand this shotgun is associated with

    //RUNTIME METHODS:
    private void Awake()
    {
        //Determine handedness:
        if (transform.parent.name.Contains("Left") || transform.parent.name.Contains("left")) handedness = Handedness.Left;                                                     //Indicate left-handedness
        else if (transform.parent.name.Contains("Right") || transform.parent.name.Contains("right")) handedness = Handedness.Right;                                             //Indicate right-handedness
        else { Debug.LogWarning("RemoteShotgunController " + name + " could not determine handedness, make sure it's parent object has the word Right or Left in its name."); } //Post error if handedness could not be determined

        //Get objects & components:
        if (!TryGetComponent(out audioSource)) audioSource = gameObject.AddComponent<AudioSource>(); //Make sure gun has audioSource
    }
    private void Start()
    {
        if (this.photonView.IsMine) //Start functionality for local client
        {
            //Get matching shotgun controller:
            foreach (PlayerEquipment equipment in PlayerController.instance.attachedEquipment) //Iterate through list of equipment attached to player
            {
                if (equipment.handedness == handedness && equipment.TryGetComponent(out clientGun)) break; //Break once matching shotgun has been found
            }
            if (clientGun == null) { Debug.LogError("RemoteShotgunController could not find matching client weapon on player, destroying self."); Destroy(gameObject); } //Post warning if client weapon could not be found

            //Initialize:
            clientGun.networkedGun = this;                                                           //Give client gun a reference to this weapon
            projectileResourceName = "Projectiles/" + clientGun.gunSettings.projectileResourceName;  //Get string for projectile resource for weapon to fire
            this.photonView.RPC("RPC_Initialize", RpcTarget.OthersBuffered, projectileResourceName); //Initialize others on network (as they buffer in)
        }
        else //Start functionality for remote clients
        {

        }
    }

    //FUNCTiONALITY METHODS:
    /// <summary>
    /// Fires this weapon on local client.
    /// </summary>
    public void LocalFire(Transform barrel)
    {
        //Initialization:
        if (!this.photonView.IsMine) //This is a remote weapon
        {
            audioSource.PlayOneShot((AudioClip)Resources.Load("Sounds/Default_Shotgun_Sound")); //Play sound on remote clients
            return;                                                                             //Ignore if this method is not being called on this client
        }

        //Launch projectile:
        Projectile projectile = PhotonNetwork.Instantiate(projectileResourceName, barrel.position, barrel.rotation).GetComponent<Projectile>(); //Instantiate projectile across network
        projectile.photonView.RPC("RPC_Fire", RpcTarget.All, barrel.position, barrel.rotation, PlayerController.photonView.ViewID);             //Initialize all projectiles simultaneously
    }

    //REMOTE METHODS:
    /// <summary>
    /// Initializes this remote weapon.
    /// </summary>
    /// <param name="projPrefabPath"></param>
    [PunRPC]
    public void RPC_Initialize(string projPrefabPath)
    {
        projectileResourceName = projPrefabPath;
    }
}
