using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class SecondaryWeapons : PlayerEquipment
{
    public GameObject blade,hand;
    public Rigidbody playerRB,bladeRB;
    public Transform headpos,attachedHand, bladeSheethed, bladeDeployed,bladeTip,rocketTip;
    public float activationTime, activationSpeed, timeAtSpeed, grindSpeed = 10, grindRange = 2, deploySpeed = 5;
    public AnimationCurve deployMotionCurve, deployScaleCurve, sheathMotionCurve, sheathScaleCurve;
    public bool deployed = false,cooldown=false,grindin=false;
    public Vector3 prevHandPos,tipPos;
    [Space()]
    [SerializeField, Range(0, 1)] private float gripThreshold = 1;

    private bool gripPressed = false;

    // Start is called before the first frame update
    private protected override void Awake()
    {
        attachedHand = hand.transform;
        //bladeRB = this.gameObject.GetComponent<Rigidbody>();
        base.Awake();
        StartCoroutine(StartCooldown());
    }
    // Update is called once per frame
    private protected override void Update()
    {

        if (blade.transform.localPosition.z >= 1.29)
        {
            bladeRB.isKinematic = true;
            bladeRB.useGravity = false;
            deployed = true;
        }

        tipPos = bladeTip.transform.position;
        Collider[] hits = Physics.OverlapSphere(tipPos, grindRange);
        grindin = false;
        foreach (var hit in hits)
        {

            if (hit.gameObject.tag != "Player"&&hit.name!="Blade")
            {
                //Debug.Log(hit.name);
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

        //if ((!deployed && forwardAngle < 90&&!cooldown) || (deployed && forwardAngle > 90&&!cooldown))
        //{
        //    handMotion = Vector3.Project(handPos - prevHandPos, hand.transform.forward);

        //    float punchSpeed = handMotion.magnitude / Time.deltaTime;
        //    if ((!deployed&&punchSpeed >= activationSpeed)||(deployed&&punchSpeed>=(activationSpeed-0.035f)))
        //    {
        //        timeAtSpeed += Time.deltaTime;
        //        if (timeAtSpeed >= activationTime)
        //        {
        //            if (!deployed)
        //            {
        //                Deploy();
        //            }
        //            else
        //            {
        //                Sheethe();
        //            }
        //        }
        //    }
        //}
        //else
        //{
        //    timeAtSpeed = 0;
        //}
        base.Update();
        prevHandPos = handPos;
    }

    private protected override void InputActionTriggered(InputAction.CallbackContext context)
    {
        if (context.action.name == "Grip")
        {
            float gripPosition = context.ReadValue<float>(); //Get current position of trigger as a value
            if (!gripPressed) //Trigger has not yet been pulled
            {
                Debug.Log(gripPosition);
                if (gripPosition >= gripThreshold) //Trigger has just been pulled
                {
                    gripPressed = true; //Indicate that trigger is now pulled
                    Deploy();
                }
            }
            else //Trigger is currently pulled
            {
                if (gripPosition < gripThreshold) //Trigger has been released
                {
                    gripPressed = false; //Indicate that trigger is now released
                    Sheethe();
                }
            }
        }
    }
    public void Deploy()
    {
        Debug.Log("Deploy");
        //blade.transform.position = bladeDeployed.position;
        bladeRB.isKinematic = false;
        bladeRB.useGravity = true;
        //  blade.transform.LookAt(bladeTip);
        blade.transform.position = Vector3.MoveTowards(blade.transform.position, bladeDeployed.transform.position, deploySpeed);
        //  bladeRB.velocity += blade.transform.forward*-deploySpeed;
        blade.transform.localRotation = bladeDeployed.transform.localRotation;
      //  deployed = true;
        StartCoroutine(StartCooldown());

    }
    public void Sheethe()
    {
        Debug.Log("Sheethe");
        blade.transform.position = Vector3.MoveTowards(blade.transform.position, bladeSheethed.transform.position, deploySpeed);
        bladeRB.isKinematic = true;
        bladeRB.useGravity = false;
       // blade.transform.position = bladeSheethed.transform.position;
        deployed = false;
        blade.transform.localRotation = bladeSheethed.transform.localRotation;
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
