using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WormHole : MonoBehaviour
{
    public Transform holePos1, holePos2,wormZone,playerHead,wormZoneShifted;
    public GameObject wormZoneParticles,wormZoneInstance,playerCam;
    public float waitTime,exitSpeed=30;
    internal bool locked = false;
    private NewShotgunController NSC;
    public PlayerController PC;
    public GameObject playerOrigin;
    public static List<WormHole> ActiveWormholes = new List<WormHole>();
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
        PC = PlayerController.instance;
        playerRB = PC.bodyRb;      
        playerCam = PC.cam.gameObject;      
        ActiveWormholes.Add(this);
        wormZoneShifted = wormZone;
        wormZoneShifted.transform.position = new Vector3(wormZone.position.x + 100 * ActiveWormholes.Count,wormZone.position.y,wormZone.position.z);
      
        playerRB.useGravity = false;                                                //Turn off Gravity
        playerRB.isKinematic = true;
        playerOBJ.transform.position = wormZoneShifted.position;
        float entryDiff = playerCam.transform.eulerAngles.y - wormZoneShifted.eulerAngles.y; //difference for player to face down wormhole
        playerOBJ.transform.rotation = Quaternion.Euler(playerOBJ.transform.eulerAngles.x, playerOBJ.transform.eulerAngles.y - entryDiff, playerOBJ.transform.eulerAngles.z);
        float startRot = playerCam.transform.eulerAngles.y;
        wormZoneInstance =Instantiate(wormZoneParticles);
        wormZoneInstance.transform.position = new Vector3(playerOBJ.transform.position.x , playerOBJ.transform.position.y, playerOBJ.transform.position.z);
        wormZoneInstance.transform.eulerAngles = new Vector3(0, startRot, 0);
        yield return new WaitForSeconds(waitTime);
        float diff = playerCam.transform.eulerAngles.y - exitPos.transform.eulerAngles.y;
        float exitDiff = playerCam.transform.eulerAngles.y - startRot;
        diff = diff - exitDiff;
        playerOBJ.transform.rotation = Quaternion.Euler(playerOBJ.transform.eulerAngles.x, playerOBJ.transform.eulerAngles.y - diff, playerOBJ.transform.eulerAngles.z);
        playerOBJ.transform.position = exitPos.position;                          
        foreach (NewGrapplerController hookController in PlayerController.instance.GetComponentsInChildren<NewGrapplerController>()) hookController.hook.Stow();
        playerRB.useGravity = true;                                                 //Bring back Gravity
        playerRB.isKinematic = false;
        playerRB.velocity = exitPos.forward * exitSpeed;                            //launch out of wormhole                                                        
        yield return new WaitForSeconds(0.2f);                                      //Wait for the player to get clear of the wormhole
        ActiveWormholes.Remove(this);
        Destroy(wormZoneInstance);
        locked = false;                                                             //Unlock the Womrhole circut
        
    }
}
