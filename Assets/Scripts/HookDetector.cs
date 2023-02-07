using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookDetector : MonoBehaviour
{
    public Rigidbody hookrb;
    public RocketBoost RBScript;
    // Start is called before the first frame update
    void Awake()
    {
        hookrb = this.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
     void Update()
    {
    }

    private void OnCollisionEnter(Collision collision)
    {
        hookrb.isKinematic = true;
        Debug.Log("hit");
        RBScript.grappleCooldown = false;
    }
}
