using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.CoreUtils;

public class JumpPad : MonoBehaviour
{
    public float jumpForce=10;
    public Transform jumpDirection;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
  
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out XROrigin playerOrigin))
        {
            print("Jump pad used by " + other.name);
            Rigidbody playerRb = playerOrigin.GetComponent<Rigidbody>();
            playerRb.transform.position = this.transform.position;
            playerRb.velocity = this.transform.up * jumpForce;
            return;
        }
    }
}
