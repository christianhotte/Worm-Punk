using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WormHoleTrigger : MonoBehaviour
{
    private WormHole WHS;
    // Start is called before the first frame update
    void Start()
    {
        WHS = gameObject.GetComponentInParent<WormHole>();
    }

    // Update is called once per frame
    void Update()
    {
       
    }


    private void OnTriggerEnter(Collider other)
    {
       // print("Wormhole hit " + other.name);
        NetworkPlayer player = other.GetComponentInParent<NetworkPlayer>();
        if (player == null) player = other.GetComponent<NetworkPlayer>();
        if (player != null && player.photonView.IsMine&&!WHS.locked)
        {
            GameObject playerRb = PlayerController.instance.bodyRb.gameObject;
            StartCoroutine(WHS.StartWormhole(this.gameObject, playerRb));
            return;
        }
        else
        {
            PlayerController playerC = other.GetComponentInParent<PlayerController>();
            if (playerC == null) playerC = other.GetComponent<PlayerController>();
            if (playerC != null&&!WHS.locked)
            {
                GameObject playerRb = PlayerController.instance.bodyRb.gameObject;
                StartCoroutine(WHS.StartWormhole(this.gameObject, playerRb));
                return;
            }
        }
    }
}
