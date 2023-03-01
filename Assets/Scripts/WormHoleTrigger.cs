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
