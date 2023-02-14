using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DavidProjectile : MonoBehaviourPun
{
    private Vector3 shootDirection;
    public float speed = 20f;
    public float lifeTime = 3f;

    // Gets the projectile's direction
    public void Initialize(Vector3 direction)
    {
        shootDirection = direction;
    }

    // Because we are not using any Rigidbodies, but this also means that it cannot bounce etc.
    private void Update()
    {
        transform.position += shootDirection * speed * Time.deltaTime;
        lifeTime -= Time.deltaTime;

        if (lifeTime <= 0)
        {
            // Destroys the projectile on the network
            photonView.RPC("RPC_DestroyProjectile", RpcTarget.All);
        }
    }

    // Might have to slap on more Colliders with *Is Trigger
    private void OnTriggerEnter(Collider other)
    {
        // Doesn't shoot from other people's guns lmao
        if (!photonView.IsMine) return;

        // When the projectile hits something with tags
        if (other.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            // Destroys the projectile on the network
            photonView.RPC("RPC_DestroyProjectile", RpcTarget.All);
        }

        // Check if the projectile has hit a network player
        if (other.gameObject.CompareTag("NetworkPlayer"))
        {
            // Do something when the projectile hits a network player, e.g. apply damage
            photonView.RPC("RPC_DestroyProjectile", RpcTarget.All);
        }
    }

    // The proper way to destroy projectiles on the network.
    [PunRPC]
    private void RPC_DestroyProjectile()
    {
        Destroy(gameObject);
    }
}