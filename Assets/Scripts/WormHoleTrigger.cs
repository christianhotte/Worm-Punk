using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
       if(exiting && !flashin)
        {
            StartCoroutine(FlashLight());
        }
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

    public IEnumerator FlashLight()
    {
        flashin = true;
        light.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        light.SetActive(false);
        flashin = false;
    }
}
