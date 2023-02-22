using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WormHole : MonoBehaviour
{
    public Transform holePos1, holePos2,wormZone,playerHead;
    public float waitTime,exitSpeed=30;
    internal bool locked = false;
    private NewShotgunController NSC;
    public GameObject playerOrigin;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public IEnumerator StartWormhole(GameObject startHole,GameObject playerOBJ)
    {
        locked = true;                                                               // Locks the worm whole circut
        Transform exitPos;                                                           //define Exit Point
        Rigidbody playerRB;
        if (holePos1.transform == startHole.transform) exitPos = holePos2.transform; //Set the exit point
        else exitPos = holePos1.transform;                                           // or the corresponding one
        //NSC = playerOBJ.GetComponentInChildren<NewShotgunController>(); 
        //NSC.locked = true;
        playerRB = playerOBJ.GetComponent<Rigidbody>();                             //Grab the Player RIgid BOdy
        playerRB.useGravity = false;                                                //Turn off Gravity
        playerOBJ.transform.position = wormZone.position;                           //Banish player to the shadow realm
        yield return new WaitForSeconds(waitTime);                                  //wait for waittime
        playerOBJ.transform.position = exitPos.position;                            // BRing player back
        playerRB.useGravity = true;                                                 //Bring back Gravity
        playerRB.velocity = exitPos.forward * exitSpeed;                            //launch out of wormhole
      //  NSC.locked = false;                                                          
        yield return new WaitForSeconds(0.2f);                                      //Wait for the player to get clear of the wormhole
        locked = false;                                                             //Unlock the Womrhole circut
        
    }
}
