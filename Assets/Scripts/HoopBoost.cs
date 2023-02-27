using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoopBoost : MonoBehaviour
{
    public Transform hoopCenter;
    private PlayerController PC;
    private Rigidbody playerRB;
    public float boostAmount,hoopRange;
    internal bool launchin = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    // Update is called once per frame
    void Update()
    {
        Collider[] frontHits = Physics.OverlapSphere(hoopCenter.position, hoopRange, LayerMask.GetMask("Player"));
        foreach (var hit in frontHits)
        {
            if (hit.name == "XR Origin") //Makes sure its not the hands
            {
                PC = hit.gameObject.GetComponentInParent<PlayerController>();
                if (!PC.Launchin)
                {
                    PC.Launchin = true;
                    StartCoroutine(HoopLaunch(hit));
                    break;
                }
            }
            else
            {
                break;
            }
        }       
    }
    public IEnumerator HoopLaunch(Collider hitPlayer)
    {
        PC = PlayerController.instance;
        playerRB = PC.bodyRb;

        Vector3 entryVel = Vector3.Project(playerRB.velocity, hoopCenter.forward);
        Vector3 exitVel = entryVel.normalized*boostAmount;
        playerRB = hitPlayer.GetComponent<Rigidbody>();
        playerRB.velocity += entryVel;
        yield return new WaitForSeconds(0.2f);
        PC.Launchin = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(hoopCenter.position, hoopRange);
    }
}
