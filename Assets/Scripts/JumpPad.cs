using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : MonoBehaviour
{
    public Rigidbody playerRb;
    public GameObject playerOBJ;
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
        // print("Wormhole hit " + other.name);
        NetworkPlayer player = other.GetComponentInParent<NetworkPlayer>();
        if (player == null) player = other.GetComponent<NetworkPlayer>();
        if (player != null && player.photonView.IsMine)
        {
            playerRb = PlayerController.instance.bodyRb;
            playerOBJ = PlayerController.instance.bodyRb.gameObject;
            playerOBJ.transform.position = jumpDirection.position;
            playerRb.velocity = jumpDirection.up * jumpForce;
            return;
        }
        else
        {
            PlayerController playerC = other.GetComponentInParent<PlayerController>();
            if (playerC == null) playerC = other.GetComponent<PlayerController>();
            if (playerC != null)
            {
                playerRb = PlayerController.instance.bodyRb;
                playerOBJ = PlayerController.instance.bodyRb.gameObject;
                playerOBJ.transform.position = jumpDirection.position;
                playerRb.velocity = jumpDirection.up * jumpForce;
                return;
            }
        }
    }
}
