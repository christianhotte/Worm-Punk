using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WormHole : MonoBehaviour
{
    public Transform holePos1, holePos2,wormZone;
    public float waitTime,exitSpeed=30;
    internal bool locked = false;
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
        locked = true;
        Transform firstPos, exitPos;
        Rigidbody playerRB;
        if (holePos1.transform == startHole.transform) exitPos = holePos2.transform;
        else exitPos = holePos1.transform;
        playerRB = playerOBJ.GetComponent<Rigidbody>();
        playerRB.useGravity = false;
        playerOBJ.transform.position = wormZone.position;
        yield return new WaitForSeconds(waitTime);
        playerOBJ.transform.position = exitPos.position;
        playerRB.useGravity = true;
        playerRB.velocity = exitPos.forward * exitSpeed;
        yield return new WaitForSeconds(0.2f);
        locked = false;
    }
}
