using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class SecondaryWeapons : PlayerEquipment
{
    public GameObject blade,hand;
    public Rigidbody playerRB;
    public Transform headpos,attachedHand, bladeSheethed, bladeDeployed,bladeTip,rocketTip;
    public float activationTime, activationSpeed,timeAtSpeed,grindSpeed=10,grindRange=2;
    public AnimationCurve deployMotionCurve, deployScaleCurve, sheathMotionCurve, sheathScaleCurve;
    public bool deployed = false,cooldown=false,grindin=false;
    public Vector3 prevHandPos,tipPos;

    // Start is called before the first frame update
    private protected override void Awake()
    {
        attachedHand = hand.transform;
        base.Awake();
        StartCoroutine(StartCooldown());
    }
    // Update is called once per frame
    private protected override void Update()
    {
        tipPos = bladeTip.transform.position;
        Collider[] hits = Physics.OverlapSphere(tipPos, grindRange);
        grindin = false;
        foreach (var hit in hits)
        {

            if (hit.gameObject.tag != "Player"&&hit.name!="Blade")
            {
                Debug.Log(hit.name);
                grindin = true;
                break;
            }
        }
        if (grindin&&deployed)
        {
            //Debug.Log("Grindin");
            playerRB.velocity = bladeDeployed.forward * grindSpeed;
        }
        Vector3 handPos,handMotion;
        handPos = headpos.InverseTransformPoint(attachedHand.position);
        handMotion = handPos - prevHandPos;
        float forwardAngle = Vector3.Angle(handMotion, transform.forward);

        if ((!deployed && forwardAngle < 90&&!cooldown) || (deployed && forwardAngle > 90&&!cooldown))
        {
            handMotion = Vector3.Project(handPos - prevHandPos, hand.transform.forward);

            float punchSpeed = handMotion.magnitude / Time.deltaTime;
            if ((!deployed&&punchSpeed >= activationSpeed)||(deployed&&punchSpeed>=(activationSpeed-0.035f)))
            {
                timeAtSpeed += Time.deltaTime;
                if (timeAtSpeed >= activationTime)
                {
                    if (!deployed)
                    {
                        Deploy();
                    }
                    else
                    {
                        Sheethe();
                    }
                }
            }
        }
        else
        {
            timeAtSpeed = 0;
        }
        base.Update();
        prevHandPos = handPos;
    }
    public void Deploy()
    {
        blade.transform.position = bladeDeployed.position;
        deployed = true;
        StartCoroutine(StartCooldown());

    }
    public void Sheethe()
    {
        blade.transform.position = bladeSheethed.transform.position;
        deployed = false;
        StartCoroutine(StartCooldown());
    }
    public IEnumerator StartCooldown()
    {
        cooldown = true;
        yield return new WaitForSeconds(1.0f);
        cooldown = false;
    }
    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Ground")
        {
            grindin = true;
        }
    }
    public void OnTriggerExit(Collider other)
    {
        if (other.tag == "Ground")
        {
            grindin = false;
        }
    }
}
