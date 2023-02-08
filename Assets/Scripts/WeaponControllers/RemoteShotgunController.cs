using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

/// <summary>
/// Governs networked shotgun firing.
/// </summary>
public class RemoteShotgunController : MonoBehaviourPunCallbacks
{
    //Objects & Components:
    internal NewShotgunController clientGun; //Client weapon associated with this networked weapon

    //Runtime Variables:

    //FUNCTiONALITY METHODS:
    /// <summary>
    /// Fires this weapon on local client.
    /// </summary>
    public void LocalFire(Transform barrel)
    {
        //Validity checks:
        if (!this.photonView.IsMine) return; //Ignore if this method is not being called on this client

        //Launch projectile:
        Projectile projectile = PhotonNetwork.Instantiate(clientGun.gunSettings.projectileResourceName, barrel.position, barrel.rotation).GetComponent<Projectile>(); //Instantiate projectile on network
        projectile.Fire(barrel);                                                                                                                                      //Indicate to projectile that it is being fired
    }
    /// <summary>
    /// Fires this weapon on matching networked weapons.
    /// </summary>
    [PunRPC]
    public void RPC_Fire(Transform barrel)
    {

    }
}
