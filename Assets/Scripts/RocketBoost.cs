using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RocketBoost : PlayerEquipment  //renametograpple
{
    public Rigidbody playerBody;
    public Transform rocketTip,rocketTipReset,hookedPos,hookStartPos;
    public GameObject HookModel,HookInstance;
    public Vector3 rayHitPoint, rocketTipStart;
    public float rocketPower=20,grappleDistance=10,releasePower=5,realDistance=0,hookSpeed=5;
    public bool Grapplin = false,grappleCooldown=true,grapplinWall=false,shootinHook=false;
    public int hitType = 0;
    public MeshRenderer Rocket;
    // Start is called before the first frame update
    private protected override void Awake()
    {
        base.Awake();
       
    }

    // Update is called once per frame
    private protected override void Update()
    {
       // if(realDistance!= null) Debug.Log(realDistance);

        if (hookedPos != null)
        {
            realDistance = Vector3.Distance(rocketTip.position, hookedPos.position); // gets distance to the hit
        }

        if (shootinHook)
        {
            Rocket.enabled = false;
            HookInstance.transform.LookAt(rayHitPoint);
            HookInstance.transform.position = Vector3.MoveTowards(HookInstance.transform.position, rayHitPoint, hookSpeed);
        }

        if (Grapplin&&!grappleCooldown)
        {
            rocketTip.LookAt(rayHitPoint);//sets grapple to look at grapple point
            Vector3 newPlayerVelocity = (rocketTip.forward * rocketPower);

            playerBody.velocity = newPlayerVelocity;//move the player
        }
        if (!grappleCooldown && realDistance < 5.5&&grapplinWall)
        {
            playerBody.velocity = (rocketTip.up * releasePower); //the bounce after grapple is released
            GrappleStop();
        }
        else if (!grappleCooldown && realDistance < 16 && !grapplinWall)
        {
            playerBody.velocity = (rocketTip.forward * releasePower); //the bounce after grapple is released
            GrappleStop();
        }
        base.Update();
    }
    public void OnGripInput(InputAction.CallbackContext context)
    {
        //Vector3 hitNormal = Vector3.up;
        //AnimationCurve bounceCurve = new AnimationCurve();

        //float groundness = 1 - (Mathf.Min(Vector3.Angle(Vector3.up, hitNormal), 90) / 90);
        //float realReleasePower = bounceCurve.Evaluate(groundness) * releasePower;

        float gripValue = context.ReadValue<float>();//value 0-1 of how much pulled
        // print("Context: " + context.phase + "| Value: " + gripValue);
        if (gripValue > 0.95 && !Grapplin)//is 95% pressed or more grapple
        {
            GrappleStart();
        }
        if (gripValue == 0 && Grapplin)//if released stop grapple
        {
            GrappleStop();
        }
        
    }
    public void GrappleStart()
    {
        if (!Grapplin)
        {
            RaycastHit hit;
            var ray = Physics.Raycast(rocketTip.position, rocketTip.forward, out hit, grappleDistance);
            if (hit.collider == null) return;

            HookInstance = Instantiate(HookModel);
            HookDetector detector = HookInstance.GetComponent<HookDetector>();
            detector.RBScript = this.gameObject.GetComponent<RocketBoost>();
            HookInstance.transform.position = hookStartPos.position;
            hookedPos = hit.transform;
            Debug.Log(hit.normal);
            rayHitPoint = hit.point;
           // StartCoroutine(GrappleLaunch());
            Grapplin = true;
            shootinHook = true;

            if (hit.normal.y!=0&&hit.normal!=Vector3.zero)//if hit ground, also stopps iff null
            {
                grapplinWall = false;
            }
            else if (hit.normal != Vector3.zero)//if hit wall, also stopps iff null
            {
                grapplinWall = true;
            }
        }
      
    }
    public void GrappleStop()
    {
        //Debug.Log("release");
        Rocket.enabled = true;
        hitType = 0;
        Rocket.enabled = true;
        Grapplin = false;
        grappleCooldown = true;
        rocketTip.LookAt(rocketTipReset);
        HookInstance.SetActive(false);


    }
    public IEnumerator GrappleLaunch()
    {
        yield return new WaitForSeconds(0.35f);//delay before grapple impulse begins after detecting hit
        Rocket.enabled = false;
        grappleCooldown = false;

    }
}
