using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : MonoBehaviour
{
    public Rigidbody playerRb;
    public float jumpForce=10;
    private float jumpPadRange;
    public Transform jumpDirection;
    // Start is called before the first frame update
    void Start()
    {
        jumpPadRange = 1;
    }

    // Update is called once per frame
    void Update()
    {
        Collider[] hits = Physics.OverlapSphere(jumpDirection.position, jumpPadRange, LayerMask.GetMask("Player"));
            foreach (var hit in hits)
            {
                playerRb = hit.gameObject.GetComponent<Rigidbody>();
                playerRb.velocity = jumpDirection.up * jumpForce;
                break;

            }
        
    }
}
