using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;

public class DavidShoot : MonoBehaviourPunCallbacks
{
    public GameObject projectilePrefab;
    private GameObject networkPlayer;
    public GameObject shootPoint;

    private void Start()
    {
        //StartCoroutine(WaitForNetworkPlayer());
        networkPlayer = GetComponentInParent<NetworkPlayer>().gameObject;

        //PlayerController.instance.input.actions.FindAction("Shoot").started += ShootProjectile;
    }

    // Checks if the Network Player has spawned yet (normally the network player isn't in the scene until connected to my rooms)
    /*private IEnumerator WaitForNetworkPlayer()
    {
        // Finds the player when the player spawns in (connects to the network)
        while (networkPlayer == null)
        {
            networkPlayer = GameObject.FindWithTag("NetworkPlayer");
            yield return null;
        }
    }*/

    // Shoots the projectile
    public void ShootProjectile()
    {
        // Only the owner of the network player object can shoot the projectile lmao
        if (!photonView.IsMine)
        {
            return;
        }

        // Send a network message to other players in the room instead of tracking the projectiles every frame through the network.
        photonView.RPC("RPC_ShootProjectile", RpcTarget.Others, networkPlayer.transform.position, networkPlayer.transform.forward);

        // Instantiate the projectile on the local client
        Vector3 shootDirection = networkPlayer.transform.forward;
        DavidProjectile projectile = PhotonNetwork.Instantiate("DavidProjectile1", shootPoint.transform.position, Quaternion.LookRotation(shootDirection), 0).GetComponent<DavidProjectile>();
        projectile.Initialize(shootDirection);
        Debug.Log("Shooting test");
    }

    // The proper way to instantiate projectiles on a network.
    [PunRPC]
    private void RPC_ShootProjectile(Vector3 shootPosition, Vector3 shootDirection)
    {
        // Instantiate the projectile on other clients based on the information in the network message
        GameObject projectile = PhotonNetwork.Instantiate(projectilePrefab.name, shootPosition, Quaternion.LookRotation(shootDirection), 0);
    }
}