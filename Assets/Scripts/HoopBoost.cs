using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoopBoost : MonoBehaviour
{
    public Transform hoopFront,hoopBack;
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
        Collider[] frontHits = Physics.OverlapSphere(hoopFront.position, hoopRange, LayerMask.GetMask("Player"));
        foreach (var hit in frontHits)
        {
            if (hit.name == "XR Origin") //Makes sure its not the hands
            {
                PC = hit.gameObject.GetComponentInParent<PlayerController>();
                if (!PC.Launchin)
                {
                    StartCoroutine(HoopLaunchFront(hit));
                    break;
                }
            }
            else
            {
                break;
            }
        }
        Collider[] backHits = Physics.OverlapSphere(hoopBack.position, hoopRange, LayerMask.GetMask("Player"));
        foreach (var hit in backHits)
        {
            if (hit.name == "XR Origin") //Makes sure its not the hands
            {
                PC = hit.gameObject.GetComponentInParent<PlayerController>();
                if (!PC.Launchin)
                {
                    StartCoroutine(HoopLaunchBack(hit));
                    break;
                }
            }
            else
            {
                break;
            }
        }
    }
    public IEnumerator HoopLaunchFront(Collider hitPlayer)
    {
        Debug.Log("FrontLaunch");
        PC = hitPlayer.gameObject.GetComponentInParent<PlayerController>();
        PC.Launchin = true;
        playerRB = hitPlayer.GetComponent<Rigidbody>();
        playerRB.velocity += hoopFront.forward * -boostAmount;
        yield return new WaitForSeconds(0.2f);
        PC.Launchin = false;

    }
    public IEnumerator HoopLaunchBack(Collider hitPlayer)
    {
        Debug.Log("BackLaunch");
        PC = hitPlayer.gameObject.GetComponentInParent<PlayerController>();
        PC.Launchin = true;
        playerRB = hitPlayer.GetComponent<Rigidbody>();
        playerRB.velocity += hoopBack.forward * -boostAmount;
        yield return new WaitForSeconds(0.2f);
        PC.Launchin = false;

    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(hoopFront.position, hoopRange);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(hoopBack.position, hoopRange);
    }
}
