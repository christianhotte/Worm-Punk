using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookDetector : MonoBehaviour
{
    public Rigidbody hookrb;
    public RocketBoost RBScript;
    public Transform hookLead;
    public float hookSpeed = 3;
    public bool flying = false;
    public LineRenderer cable;

    // Start is called before the first frame update
    void Awake()
    {
        hookrb = this.GetComponent<Rigidbody>();
        Physics.IgnoreCollision(PlayerController.instance.bodyRb.GetComponent<Collider>(), GetComponent<Collider>());

        flying = true;
        cable = this.gameObject.AddComponent<LineRenderer>();
        cable.startColor = Color.black;
        cable.endColor = Color.black;
        cable.material = new Material(Shader.Find("Sprites/Default"));
        cable.startWidth = 0.01f;
        cable.endWidth = 0.01f;
    }

    // Update is called once per frame
    void Update()
    {
        if (flying)
        {
           // this.transform.LookAt(hookLead);
            hookrb.velocity = (RBScript.rocketTip.forward * hookSpeed);
           // this.transform.position = Vector3.MoveTowards(this.transform.position, hookLead.transform.position, hookSpeed);
            if (this.gameObject.GetComponent<LineRenderer>() != null)
            {

                cable.SetPosition(0, RBScript.rocketTip.transform.position);
                cable.SetPosition(1, this.transform.position);
            }
        }
       
    }

    private void OnCollisionEnter(Collision collision)
    {
        hookrb.isKinematic = true;
        flying = false;
        Debug.Log("hit object " + collision.collider.name + " on Layer " + collision.collider.gameObject.layer);
        RBScript.grappleCooldown = false;
        RBScript.HookHit();
    }
}
