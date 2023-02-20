using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// New controller for player shotgun, fires a single slug.
/// </summary>
public class NewShotgunController : PlayerEquipment
{
    //Objects & Components:
    internal ConfigurableJoint breakJoint;         //Joint controlling weapon's break action
    internal RemoteShotgunController networkedGun; //Remote weapon script used to fire guns on the network
    internal NewShotgunController otherGun;        //Shotgun in the player's other hand

    private ParticleSystem shotParticles; //Particle system which activates each time weapon is fired

    //Settings:
    [SerializeField, Tooltip("Transforms representing position and direction of weapon barrels.")] private Transform[] barrels;
    [Tooltip("Settings object which determines general weapon behavior.")]                         public ShotgunSettings gunSettings;
    [Space()]
    [SerializeField, Tooltip("Makes it so that weapon fires from the gun itself and not on the netwrok.")] private bool debugFireLocal = false;

    //Runtime Variables:
    private int currentBarrelIndex = 0; //Index of barrel currently selected as next to fire
    private int loadedShots = 0;        //Number of shots weapon is able to fire before needing to reload again
    internal bool breachOpen = false;   //Indicates whether or not weapon breach is swung open
    private float breachOpenTime = 0;   //Time breach has been open for (zero if breach is closed)
    private bool triggerPulled = false; //Whether or not the trigger is currently pulled
    private float doubleFireWindow = 0; //Above zero means that weapon has just been fired and firing another weapon will cause a double fire
    internal bool locked = false;       //Lets the other equipment disable the guns
    private Vector3 baseScale;          //Initial scale of weapon

    //Events & Coroutines:
    /// <summary>
    /// Weapon recoil procedure (triggers a number of effects throughout recoil phase).
    /// </summary>
    public IEnumerator DoRecoil()
    {
        //Initialize:
        rb.maxAngularVelocity = gunSettings.recoilAngularSpeed;        //Make weapon very swingy for recoil
        Vector3 maxOffset = gunSettings.recoilDistance * Vector3.back; //Get greatest offset value which weapon will reach during recoil phase
        Vector3 maxScale = gunSettings.recoilScale * baseScale;        //Get greatest scale value which weapon will reach during recoil phase

        //Move weapon:
        for (float totalTime = 0; totalTime < gunSettings.recoilTime; totalTime += Time.fixedDeltaTime) //Iterate once each fixed update for duration of recoil phase
        {
            float timeValue = totalTime / gunSettings.recoilTime;                                                         //Get value representing progression through recoil phase
            rb.maxAngularVelocity = Mathf.Lerp(gunSettings.recoilAngularSpeed, jointSettings.maxAngularSpeed, timeValue); //Adjust max angular speed back to normal throughout phase
            currentAddOffset = Vector3.Lerp(Vector3.zero, maxOffset, gunSettings.recoilCurve.Evaluate(timeValue));        //Modify follower offset so that weapon is moved backwards/forwards
            transform.localScale = Vector3.Lerp(baseScale, maxScale, gunSettings.recoilScaleCurve.Evaluate(timeValue));   //Adjust scale based on settings and curve
            yield return new WaitForFixedUpdate();                                                                        //Wait for next update
        }

        //Cleanup:
        rb.maxAngularVelocity = jointSettings.maxAngularSpeed; //Set angular speed cap back to default
        currentAddOffset = Vector3.zero;                       //Return system to base position
        transform.localScale = baseScale;                      //Reset weapon to base scale
    }

    //RUNTIME METHODS:
    private protected override void Awake()
    {
        //Validation & component get:
        if (barrels.Length == 0) Debug.LogWarning("Shotgun " + " has no assigned barrel transforms!");                                                                        //Warn player if no barrels have been assigned to shotgun
        breakJoint = GetComponentInChildren<ConfigurableJoint>(); if (breakJoint == null) { Debug.LogWarning("Shotgun does not have Configurable Joint for break action!"); } //Get configurable break joint (before spawning another one in base method)
        shotParticles = GetComponentInChildren<ParticleSystem>();                                                                                                             //Get particle system from children (fix this if multiple systems end up getting added)
        base.Awake();                                                                                                                                                         //Run base awake method

        //Check settings:
        if (gunSettings == null) //No weapon settings were provided
        {
            Debug.Log("Weapon " + name + " is missing Gun Settings, using system defaults.");        //Log warning in case someone forgot
            gunSettings = (ShotgunSettings)Resources.Load("DefaultSettings/DefaultShotgunSettings"); //Use default settings from Resources
        }
        loadedShots = gunSettings.maxLoadedShots; //Fully load weapon on start

        //Get runtime variables:
        baseScale = transform.localScale; //Get base scale
    }
    private void Start()
    {
        //Late component get:
        if (player != null) //Only try this if weapon is attached to a player
        {
            foreach (PlayerEquipment equipment in player.attachedEquipment) //Iterate through equipment currently attached to player
            {
                if (equipment.TryGetComponent(out NewShotgunController other) && other != this) otherGun = other; //Try to get other shotgun controller
            }
        }
    }
    private protected override void Update()
    {
        //Initialization:
        base.Update(); //Run base update method

        //Update timers:
        if (breachOpen && loadedShots < gunSettings.maxLoadedShots) //Breach is currently open and weapon has not been loaded
        {
            breachOpenTime = Mathf.Min(breachOpenTime + Time.deltaTime, gunSettings.cooldownTime); //Increment time tracker and max out at cooldown time
            if (breachOpenTime >= gunSettings.cooldownTime) Reload();                              //Reload weapon once cooldown time has been reached
        }
        if (doubleFireWindow > 0) doubleFireWindow = Mathf.Max(doubleFireWindow - Time.deltaTime, 0); //Decrement time tracker and floor at zero

        //Check for swing-closing:
        if (breachOpen) //Weapon breach is currently open
        {
            //NOTE: Make sure this works
            float closeForce = gunSettings.closerForce * Time.deltaTime * Vector3.Project(RelativeVelocity, targetTransform.up).magnitude;                                      //Get base force to apply to hinge
            breakJoint.GetComponent<Rigidbody>().AddForceAtPosition(barrels[currentBarrelIndex].up * closeForce, barrels[currentBarrelIndex].position, ForceMode.Acceleration); //Apply upward torque to hinged part
        }
    }

    //INPUT METHODS:
    private protected override void InputActionTriggered(InputAction.CallbackContext context)
    {
        //Determine input target:
        switch (context.action.name) //Determine behavior depending on action name
        {
            case "Trigger":
                float triggerPosition = context.ReadValue<float>(); //Get current position of trigger as a value
                if (!triggerPulled) //Trigger has not yet been pulled
                {
                    if (triggerPosition >= gunSettings.triggerThreshold) //Trigger has just been pulled
                    {
                        triggerPulled = true; //Indicate that trigger is now pulled
                        Fire();               //Begin firing sequence
                    }
                }
                else //Trigger is currently pulled
                {
                    if (triggerPosition < gunSettings.triggerThreshold) //Trigger has been released
                    {
                        triggerPulled = false; //Indicate that trigger is now released
                    }
                }
                break;
            case "BButton":
                if (context.started && !breachOpen) //Button has just been pressed and breach is currently closed
                {
                    Eject(); //Open breach and eject shells
                }
                break;
            default: break; //Ignore unrecognized actions
        }
    }

    /// <summary>
    /// Shoots the gun (instantiates projectiles in network if possible).
    /// </summary>
    public Projectile Fire()
    {
        //Validation:
        Projectile projectile = null;                           //Initialize reference to projectile
        if (loadedShots <= 0) { DryFire(); return projectile; } //Dry-fire if weapon is out of shots
        if (breachOpen) { DryFire(); return projectile; }       //Dry-fire if weapon breach is open
        if (locked) return projectile;                          //Return if locked by another weapon
        //Initialization:
        Transform currentBarrel = barrels[currentBarrelIndex]; //Get reference to active barrel

        //Rigidbody effects:
        float effectiveFireVel = gunSettings.fireVelocity;                                                             //Store fire velocity so it can be optionally modified
        if (otherGun != null && otherGun.doubleFireWindow > 0) effectiveFireVel *= gunSettings.doubleFireBoost;        //Apply boost if player is firing both weapons simultaneously
        if (player != null) player.bodyRb.velocity = -currentBarrel.forward * effectiveFireVel;                        //Launch player based on current barrel facing direction
        rb.AddForceAtPosition(currentBarrel.up * gunSettings.recoilTorque, currentBarrel.position, ForceMode.Impulse); //Apply upward torque to weapon at end of barrel

        //Instantiate projectile(s):
        if (networkedGun == null || debugFireLocal) //Weapon is in local fire mode
        {
            projectile = ((GameObject)Instantiate(Resources.Load("Projectiles/" + gunSettings.projectileResourceName))).GetComponent<Projectile>(); //Instantiate projectile
            projectile.FireDumb(currentBarrel);                                                                                                     //Initialize projectile
        }
        else networkedGun.LocalFire(currentBarrel); //Fire weapon on the network

        //Other effects:
        SendHapticImpulse(gunSettings.fireHaptics);                                //Play haptic impulse
        if (player != null) player.ShakeScreen(gunSettings.fireScreenShake);       //Shake screen (gently)
        shotParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); //Reset particle system
        shotParticles.Play();                                                      //Play particle effect
        StartCoroutine(DoRecoil());                                                //Begin recoil phase

        //Cleanup:
        doubleFireWindow = gunSettings.doubleFireTime; //Open double fire window so that other weapon can check for it
        if (gunSettings.fireSound != null) audioSource.PlayOneShot(gunSettings.fireSound); //Play sound effect
        if (barrels.Length > 1) //Only index barrel if there is more than one
        {
            currentBarrelIndex += 1;                                          //Index current barrel number by one
            if (currentBarrelIndex >= barrels.Length) currentBarrelIndex = 0; //Overflow barrel index if relevant
        }
        loadedShots = Mathf.Max(loadedShots - 1, 0); //Spend one shot (floor at zero)
        return projectile;
    }
    /// <summary>
    /// Opens weapon breach and ejects shells.
    /// </summary>
    public void Eject()
    {
        //Validity checks:
        if (breachOpen||locked) //Breach is already open or gun is locked
        {
            //SOUND EFFECT
            return; //Ignore everything else
        }

        //Open joint:
        breakJoint.angularXMotion = ConfigurableJointMotion.Limited;                          //Unlock pivot rotation
        breakJoint.targetRotation = Quaternion.Euler(Vector3.right * gunSettings.breakAngle); //Set target joint rotation to break angle
        SoftJointLimit newJointLimit = breakJoint.highAngularXLimit;                          //Copy current joint limit setting
        newJointLimit.limit = gunSettings.breakAngle;                                         //Set break angle to open position
        breakJoint.highAngularXLimit = newJointLimit;                                         //Apply new joint limit

        //Cleanup:
        if (gunSettings.ejectSound != null) audioSource.PlayOneShot(gunSettings.ejectSound); //Play sound effect
        SendHapticImpulse(gunSettings.ejectHaptics);                                         //Play haptic impulse
        breachOpen = true;                                                                   //Indicate that breach is now open
    }
    /// <summary>
    /// Closes weapon breach and makes weapon prepared to fire.
    /// </summary>
    public void Close()
    {
        //Validity checks:
        if (!breachOpen) return; //Do not attempt to close if breach is open

        //Close joint:
        breakJoint.angularXMotion = ConfigurableJointMotion.Locked;  //Lock pivot rotation
        breakJoint.targetRotation = Quaternion.Euler(Vector3.zero);  //Set target angle of break joint to zero
        SoftJointLimit newJointLimit = breakJoint.highAngularXLimit; //Copy current joint limit setting
        newJointLimit.limit = 0;                                     //Set break angle to closed value
        breakJoint.highAngularXLimit = newJointLimit;                //Apply new joint limit

        //Cleanup:
        if (gunSettings.lockSound != null) audioSource.PlayOneShot(gunSettings.lockSound); //Play sound effect
        SendHapticImpulse(gunSettings.closeHaptics);                                       //Play haptic impulse
        breachOpenTime = 0;                                                                //Reset breach open time tracker
        breachOpen = false;                                                                //Indicate that breach is now closed
    }
    /// <summary>
    /// Fully reloads weapon to max ammo capacity.
    /// </summary>
    public void Reload()
    {
        //Cleanup:
        currentBarrelIndex = 0;                   //Reset barrel index
        loadedShots = gunSettings.maxLoadedShots; //Reset shot counter to maximum
    }

    //FUNCTIONALITY METHODS:
    /// <summary>
    /// Called whenever player tries to fire weapon but weapon cannot be fired for some reason.
    /// </summary>
    public void DryFire()
    {

    }
}
