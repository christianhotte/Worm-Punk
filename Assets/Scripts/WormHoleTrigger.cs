using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.CoreUtils;

public class WormHoleTrigger : MonoBehaviour
{
    private WormHole WHS;
    internal bool exiting = false,flashin=false;
    public GameObject light;
    // Start is called before the first frame update
    void Start()
    {
        WHS = gameObject.GetComponentInParent<WormHole>();
    }

    // Update is called once per frame
    void Update()
    {
       if(exiting && !flashin)  //If someones coming out of this hole and the light isnt on
        {
            StartCoroutine(FlashLight());
        }
    }


    private void OnTriggerEnter(Collider other)
    {
     
        if (other.TryGetComponent(out XROrigin playerOrigin) && !WHS.locked) // make sure it hit the player, and the wormhole isnt locked
        {
            GameObject playerRb = PlayerController.instance.bodyRb.gameObject;//gets player reference to send to the wormhole script
            StartCoroutine(WHS.StartWormhole(this.gameObject, playerRb)); //Tells the wormhole to start the loop 
            return;
        }
    }

    public IEnumerator FlashLight()
    {
        flashin = true;
        light.SetActive(true);//turns light off
        yield return new WaitForSeconds(0.2f);
        light.SetActive(false);//turns light back on
        flashin = false;
    }
}
