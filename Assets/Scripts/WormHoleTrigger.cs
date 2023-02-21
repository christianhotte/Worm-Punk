using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WormHoleTrigger : MonoBehaviour
{
    private float wormEnterRange;
    public Transform entrancePos;
    private GameObject playerOBJ;
    private WormHole WHS;
    // Start is called before the first frame update
    void Start()
    {
        wormEnterRange = 2;
        WHS = gameObject.GetComponentInParent<WormHole>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!WHS.locked)
        {
            Collider[] hits = Physics.OverlapSphere(entrancePos.position, wormEnterRange, LayerMask.GetMask("Player"));
            foreach (var hit in hits)
            {
                if(hit.name =="XR Origin") //Makes sure its not the hands
                {
                    playerOBJ = hit.gameObject;
                    StartCoroutine(WHS.StartWormhole(this.gameObject, playerOBJ));

                    break;
                }
                else
                {
                    break;
                }

            }
        }
       
    }
   

}
