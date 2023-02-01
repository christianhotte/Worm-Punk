using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrelLock : MonoBehaviour
{
    public FirstGunScript fgs;
    // Start is called before the first frame update
    void Start()
    {
        fgs = GetComponentInParent<FirstGunScript>();
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void OnTriggerEnter(Collider other)
    {
       // Debug.Log("oof");
        if (other.gameObject.tag == "Barrel" && fgs.Ejecting)
        {
          //  Debug.Log("shutting");
            fgs.CloseBreach();
        }
    }
}
