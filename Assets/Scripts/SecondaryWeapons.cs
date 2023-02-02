using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class SecondaryWeapons : PlayerEquipment
{
    public GameObject bladde,hand;
    // public XROrigin player;
    public Transform headpos,attachedHand;
    public float activationTime, activationSpeed, deployStrength, deployTime,timeAtSpeed;
    public AnimationCurve deployMotionCurve, deployScaleCurve, sheathMotionCurve, sheathScaleCurve;
    public bool deployed = false,cooldown=false;
    public Vector3 prevHandPos;

    // Start is called before the first frame update
    private protected override void Awake()
    {
        //player = GetComponentInParent<XROrigin>();
        attachedHand = hand.transform;
        base.Awake();
        StartCoroutine(StartCooldown());
    }
    // Update is called once per frame
    private protected override void Update()
    {
        Vector3 handPos,handMotion;
        handPos = headpos.InverseTransformPoint(attachedHand.position);
        handMotion = handPos - prevHandPos;
        float forwardAngle = Vector3.Angle(handMotion, transform.forward);

        if ((!deployed && forwardAngle < 90&&!cooldown) || (deployed && forwardAngle > 90&&!cooldown))
        {
           // Debug.Log("check1");
            handMotion = Vector3.Project(handPos - prevHandPos, hand.transform.forward);

            float punchSpeed = handMotion.magnitude / Time.deltaTime;
           // Debug.Log(punchSpeed);
            if (punchSpeed >= activationSpeed)
            {
               // Debug.Log("check2");
                timeAtSpeed += Time.deltaTime;
                if (timeAtSpeed >= activationTime)
                {
                  //  Debug.Log("check3");
                    if (!deployed)
                    {
                        Debug.Log("trieddeploy");
                        Deploy();
                    }
                    else
                    {
                        Debug.Log("triedsheethe");
                        Sheethe();
                        //sheethe
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
        deployed = true;
        StartCoroutine(StartCooldown());

    }
    public void Sheethe()
    {
        deployed = false;
        StartCoroutine(StartCooldown());
    }
    public IEnumerator StartCooldown()
    {
        cooldown = true;
        yield return new WaitForSeconds(1.5f);
        cooldown = false;
    }
}
