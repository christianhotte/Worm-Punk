using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoopBoost : MonoBehaviour
{
    public Transform hoopCenter;
    private PlayerController PC;
    private Rigidbody playerRB;
    public float boostAmount;
    internal bool launchin = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    // Update is called once per frame
    void Update()
    {
          
    }
    public IEnumerator HoopLaunch(Collider hitPlayer)
    {
        Debug.Log("boostCalled");
        launchin = true;
        PC = PlayerController.instance;
        playerRB = PC.bodyRb;

        Vector3 entryVel = Vector3.Project(playerRB.velocity, hoopCenter.forward);
        Vector3 exitVel = entryVel.normalized*boostAmount;
        playerRB = hitPlayer.GetComponent<Rigidbody>();
        playerRB.velocity += entryVel;
        yield return new WaitForSeconds(0.2f);
        launchin = false;
    }

}
