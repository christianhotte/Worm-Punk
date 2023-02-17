using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using Photon;
using Photon.Pun;
public class SecondaryWeapons : PlayerEquipment
{
    public GameObject blade,hand,ProjectilePrefab,energyBlade,rightGun;
    public GameObject[] StoredShots;
    public Rigidbody playerRB;
    public Transform headpos,attachedHand, bladeSheethed, bladeDeployed,bladeTip,stowedTip,rayStartPoint,bulletSpredPoint,bladeImpulsePosition,EnergyBladeStowed,EnergyBladeExtended;
    public float activationTime, activationSpeed, timeAtSpeed, grindSpeed = 10, grindRange = 2, deploySpeed = 5,blockRadius=4,sawDistance,rayHitDistance,maxSpreadAngle=4,energySpeed=5, maxPossibleHandSpeed=10, minPossibleHandSpeed= 1,maxBladeReductSpeed = 1,explosiveForce =5;
    public AnimationCurve deployMotionCurve, deployScaleCurve, sheathMotionCurve, sheathScaleCurve;
    public bool deployed = false,cooldown=false,grindin=false,deflectin=false;
    public Vector3 prevHandPos, tipPos, storedScale, energyBladeBaseScale, energyTargetScale,energyCurrentScale,energyBladeStartSize;
    [Space()]
    [SerializeField, Range(0, 1)] private float gripThreshold = 1;
    public Projectile projScript;
    private NewShotgunController NSC;
    private bool gripPressed = false,shootin=false,stabbin=false;
    public int shotsHeld = 0, shotCap = 3,shotsToFire,shotsCharged=0;
    public AudioSource sawAud;
    public AudioClip punchSound,chainsawDeploy,chainsawSheethe;
    int num;
    private float prevInterpolant;

    // Start is called before the first frame update
    private protected override void Awake()
    {
        attachedHand = hand.transform;
        sawAud = this.GetComponent<AudioSource>();
        energyBladeBaseScale = energyBlade.transform.localScale;
        energyBlade.transform.localScale = energyBladeStartSize;
        //bladeRB = this.gameObject.GetComponent<Rigidbody>();
        base.Awake();
        NSC = rightGun.GetComponent<NewShotgunController>();
        //StartCoroutine(StartCooldown());
    }
    // Update is called once per frame
    private protected override void Update()
    {


        if (shotsToFire > 0 && !shootin)
        {
            StartCoroutine(ShootAbsorbed());
        }
        if (deployed)
        {
            //Debug.Log("checkstart");

            tipPos = bladeTip.transform.position;
            GameObject[] bullethits = GameObject.FindGameObjectsWithTag("Bullet");
            foreach (var hit in bullethits)
            {
                float bulletDistance = Vector3.Distance(bladeTip.position, hit.transform.position);
                projScript = hit.gameObject.GetComponent<Projectile>();
                if (bulletDistance <= blockRadius&&shotsHeld<shotCap)
                {
                    // Debug.Log(bulletDistance);
                    if (projScript.originPlayerID == PlayerController.photonView.ViewID) return;
                    //grindin = true;
                    Destroy(hit);
                    shotsHeld++;
                    StoredShots[shotsHeld - 1].SetActive(true);

                    break;
                }
            }
        }
        tipPos = bladeTip.transform.position;
        Collider[] hits = Physics.OverlapSphere(tipPos, grindRange, ~LayerMask.GetMask("PlayerWeapon", "Player", "Bullet", "EnergyBlade","Blade", "Hitbox"));
        grindin = false;
        foreach (var hit in hits)
        {

               Debug.Log(hit.name);
                grindin = true;
                break;
            
        }
        if (deployed)
        {
            sawDistance = Vector3.Distance(rayStartPoint.position, bladeTip.position);
            var sawRay = Physics.Raycast(rayStartPoint.position, rayStartPoint.forward, out RaycastHit checkBlade, sawDistance + 1, ~LayerMask.GetMask("PlayerWeapon", "Blade"));// ~LayerMask.GetMask("PlayerWeapon"));
            if (checkBlade.collider == null) return;
            rayHitDistance = 999;
            rayHitDistance = Vector3.Distance(rayStartPoint.position, checkBlade.point);
            if (rayHitDistance < sawDistance&&checkBlade.collider.tag!="Blade"&&checkBlade.collider.tag!="Player"&&checkBlade.collider.tag!="Barrel")
            {
              //  Debug.Log(checkBlade.collider.name);
                grindin = true;
            }
            else if (rayHitDistance > sawDistance)
            {
                grindin = false;

            }

        }

        if (grindin&&deployed)
        {
            //Debug.Log("Grindin");
            playerRB.velocity = bladeImpulsePosition.forward * grindSpeed;
        }
        Vector3 handPos,handMotion;
        handPos = attachedHand.localPosition; //headpos.InverseTransformPoint(attachedHand.position);
        handMotion = handPos - prevHandPos;
        float punchSpeed = handMotion.magnitude / Time.deltaTime;
      //  Debug.Log(punchSpeed);
        //if (deployed && punchSpeed >= activationSpeed&&!stabbin)
        //{
        //    storedScale = energyBlade.transform.localScale;
        //    stabbin = true;
        //    energyBlade.SetActive(true);
        //    for (; shotsHeld > 0; shotsHeld--){
        //        shotsCharged++;
        //        Vector3 currentScale = energyBlade.transform.localScale;
        //      //  energyBlade.transform.localScale = new Vector3(currentScale.x, currentScale.y * 1.2f, currentScale.z);
        //        StoredShots[shotsHeld - 1].SetActive(false);
        //    }

        //    StartCoroutine(BladeSlice());
        //}
        if (deployed)
        {
            NSC.locked = true;
            energyBlade.SetActive(true);
            float targetInterpolant = Mathf.Clamp01(Mathf.InverseLerp(minPossibleHandSpeed, maxPossibleHandSpeed, punchSpeed));
            if (targetInterpolant < prevInterpolant) targetInterpolant = Mathf.MoveTowards(prevInterpolant, targetInterpolant, maxBladeReductSpeed * Time.deltaTime);

            Vector3 targetPosition = Vector3.Lerp(EnergyBladeStowed.position, EnergyBladeExtended.position, targetInterpolant);
            Vector3 targetScale = Vector3.Lerp(EnergyBladeStowed.localScale, EnergyBladeExtended.localScale, targetInterpolant);
           // energyBlade.transform.localPosition = Vector3.Lerp(energyBlade.transform.position, targetPosition, energySpeed);
            energyBlade.transform.position = targetPosition;
            energyBlade.transform.localScale = targetScale;
            // energyBlade.transform.localScale = targetScale;

            //Vector3.Lerp(energyBlade.transform.localScale, targetScale, energySpeed * Time.deltaTime);
            prevInterpolant = targetInterpolant;
        }
        else
        {
            NSC.locked = false;
            energyBlade.transform.position = EnergyBladeStowed.position; //Vector3.Lerp(energyBlade.transform.localScale, energyBladeStartSize, energySpeed);
            energyBlade.transform.localScale = EnergyBladeStowed.localScale; //Vector3.Lerp(energyBlade.transform.localScale, energyBladeStartSize, energySpeed);
            energyBlade.SetActive(false);
        }
        prevHandPos = handPos;
        //float forwardAngle = Vector3.Angle(handMotion, transform.forward);

        //if (forwardAngle < 90&&!cooldown) //|| (deployed && forwardAngle > 90&&!cooldown))                   Code for punch detection
        //{

        //    handMotion = Vector3.Project(handPos - prevHandPos, hand.transform.forward);

        //    float punchSpeed = handMotion.magnitude / Time.deltaTime;

        //    if ((!deployed&&punchSpeed >= activationSpeed))
        //    {


        //                // Deploy();

        //         num++;
        //        // Debug.Log("Punch"+num);
        //        sawAud.PlayOneShot(punchSound);
        //        StartCoroutine(DeflectTime());
        //        StartCoroutine(StartCooldown());


        //    }
        //}
        //else
        //{
        //   // timeAtSpeed = 0;
        //}
        //base.Update();
        //prevHandPos = handPos;
    }

    private protected override void InputActionTriggered(InputAction.CallbackContext context)
    {
        if (context.action.name == "Grip")
        {
            float gripPosition = context.ReadValue<float>(); //Get current position of trigger as a value
            if (!gripPressed) //Trigger has not yet been pulled
            {
               // Debug.Log(gripPosition);
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
       // Debug.Log("Deploy");
        //  blade.transform.LookAt(bladeTip);
        blade.transform.position = Vector3.MoveTowards(blade.transform.position, bladeDeployed.transform.position, deploySpeed);
        //  bladeRB.velocity += blade.transform.forward*-deploySpeed;
        blade.transform.localRotation = bladeDeployed.transform.localRotation;
        deployed = true;
        sawAud.PlayOneShot(chainsawDeploy);
        
        StartCoroutine(StartCooldown());

    }
    public void Sheethe()
    {
       // Debug.Log("Sheethe");
        blade.transform.position = Vector3.MoveTowards(blade.transform.position, bladeSheethed.transform.position, deploySpeed);

       // blade.transform.position = bladeSheethed.transform.position;
        deployed = false;
        sawAud.PlayOneShot(chainsawSheethe);
        //blade.transform.localRotation = bladeSheethed.transform.localRotation;
        StartCoroutine(StartCooldown());
        if (shotsHeld > 0)
        {
            ClearAbsorbed();
        }
    }
    public IEnumerator StartCooldown()
    {
        cooldown = true;
        yield return new WaitForSeconds(1.0f);
        cooldown = false;
    }
    public IEnumerator ShootAbsorbed()
    {
        shootin = true;
        for (; shotsToFire > 0; shotsToFire--)
        {
            GameObject projInstance = Instantiate(ProjectilePrefab);
            Vector3 exitAngles = Random.insideUnitCircle * maxSpreadAngle;
            bulletSpredPoint.localEulerAngles = new Vector3(bladeTip.position.x + exitAngles.x, bladeTip.position.y + exitAngles.y, bladeTip.position.z + exitAngles.z);
            projInstance.transform.position = bulletSpredPoint.position;
            projInstance.transform.rotation = bulletSpredPoint.rotation;
            projScript = projInstance.GetComponent<Projectile>();
            projScript.Fire(bulletSpredPoint.position, bladeTip.rotation);
            StoredShots[shotsToFire - 1].SetActive(false);
            yield return new WaitForSeconds(.05f);
        }
        shootin = false;
    }
    public void ClearAbsorbed()
    {
        float prevExplosiveForce=explosiveForce;
        for(; shotsHeld > 0; shotsHeld--)
        {
            StoredShots[shotsHeld - 1].SetActive(false);
            explosiveForce *= 1.5f;
        }
        //  playerRB.AddExplosionForce(explosiveForce,bladeImpulsePosition.position,2);
        playerRB.velocity = stowedTip.forward * -explosiveForce;
        explosiveForce = prevExplosiveForce;
    }
    public IEnumerator DeflectTime()
    {
        deflectin = true;
        yield return new WaitForSeconds(0.3f);
        deflectin = false;
       // yield return new WaitForSeconds(0.2f);
         Sheethe();
    }
    public IEnumerator BladeSlice()
    {
       
        yield return new WaitForSeconds(1.5f);
        energyBlade.SetActive(false);
       
        // yield return new WaitForSeconds(5.0f);
        energyBlade.transform.localScale = energyBladeStartSize;
        stabbin = false;
    }
    //private void OnTriggerEnter(Collider other)
    //{
    //    NetworkPlayer targetPlayer = other.GetComponentInParent<NetworkPlayer>();     //Try to get network player from hit collider
    //    if (targetPlayer == null) targetPlayer = other.GetComponent<NetworkPlayer>(); //Try again for network player if it was not initially gotten
    //    if (targetPlayer != null)
    //    {
    //        other.GetComponent<NetworkPlayer>().photonView.RPC("RPC_Hit", RpcTarget.All, 5);
    //        sawAud.PlayOneShot(punchSound);
    //    }
    //}
}
