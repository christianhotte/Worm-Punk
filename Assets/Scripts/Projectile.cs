using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Pun;
using System.Linq;

/// <summary>
/// Base class for any non-hitscan ballistic projectiles.
/// </summary>
public class Projectile : MonoBehaviourPunCallbacks
{
    //Objects & Components:

    //Settings:
    [Tooltip("Settings object determining base properties of projectile.")] public ProjectileSettings settings;

    //Runtime Variables:
    private Vector3 velocity;              //Speed and direction at which projectile is traveling
    private PlayerController originPlayer; //The player which shot this projectile
    private Transform target;              //Transform which projectile is currently homing toward
    private float totalDistance;           //Total travel distance covered by this projectile

    internal bool localOnly = false; //Indicates that this projectile does not have equivalents on the network

    //Utility Variables:
    /// <summary>
    /// How much distance this projectile has left to cover.
    /// </summary>
    private float RemainingRange { get { return settings.range - totalDistance; } }
    /// <summary>
    /// Percentage of distance which has been traveled by this projectile so far.
    /// </summary>
    private float DistancePercent { get { return 1 - (RemainingRange / settings.range); } }

    //COROUTINES:
    /// <summary>
    /// Performs target acquisition check on set timestep until projectile has its desired target (also resets targeting state).
    /// </summary>
    /// <returns></returns>
    IEnumerator DoTargetAcquisition()
    {
        //Initialization:
        target = null;                                            //Reset current target
        float secsPerUpdate = 1 / settings.targetingTickRate;     //Get seconds per tick
        List<Transform> potentialTargets = new List<Transform>(); //Create list for storing viable targets
        float targetHeuristic = 0;                                //Heuristic value for current target (the higher the better)

        //Populate potential targets list:
        var targetables = FindObjectsOfType<MonoBehaviour>().OfType<IShootable>().OfType<MonoBehaviour>(); //Get master list of all targetable objects in scene
        foreach (MonoBehaviour targetable in targetables) //Iterate through list of targetables
        {
            //Eliminate non-viable targets:
            if (targetable == originPlayer) continue;                               //Prevent projectile from targeting origin player
            Vector3 targetSep = targetable.transform.position - transform.position; //Get distance and direction from projectile to target
            float targetDist = targetSep.magnitude;                                 //Distance from projectile to target
            if (targetDist > RemainingRange) continue;                              //Ignore targets which are outside projectile's potential range
            float targetAngle = Vector3.Angle(targetSep, transform.forward);        //Get angle between target direction and projectile movement direction
            if (targetAngle > settings.targetDesignationAngle.y) continue;          //Ignore targets which are behind the projectile and will likely never be hit

            //Cleanup:
            potentialTargets.Add(targetable.transform); //Add valid targets to list of targets to check
        }
        print("Potential Targets: " + potentialTargets.Count);
        

        //Look for targets:
        while (true) //Run forever
        {
            foreach (Transform potentialTarget in potentialTargets)
            {
                //Eliminate non-viable targets:
                Vector3 targetSep = potentialTarget.position - transform.position; //Get distance and direction from projectile to target
                float targetDist = targetSep.magnitude;                            //Distance from projectile to target
                if (targetDist > RemainingRange) continue;                         //Ignore targets which are outside projectile's potential range
                float targetAngle = Vector3.Angle(targetSep, transform.forward);   //Get angle between target direction and projectile movement direction
                if (targetAngle > settings.targetDesignationAngle.y) continue;     //Ignore targets which are behind the projectile and will likely never be hit

                //Check for viable targets:
                if (targetDist <= settings.targetingDistance) //Current targetable is within range of projectile
                {
                    //Check for obstructions:
                    if (settings.LOSTargeting) //System is using line-of-sight targeting
                    {
                        if (Physics.Linecast(transform.position, potentialTarget.position, settings.targetingIgnoreLayers)) //Targetable is obstructed and is not currently valid
                        {
                            potentialTargets.Add(potentialTarget.transform); //Add targetable to list of potential targets
                            continue;                                        //Skip everything else
                        }
                    }

                    //Check target viability:
                    float currentHeuristic = (1 - settings.angleDistancePreference) * Mathf.InverseLerp(settings.targetDesignationAngle.x, 0, targetAngle); //Calculate angle preference score for this target
                    currentHeuristic += settings.angleDistancePreference * Mathf.InverseLerp(settings.targetingDistance, 0, targetDist);                    //Calculate proximity preference score for this target
                    if (target == null && targetAngle <= settings.targetDesignationAngle.x || //No target has been chosen and target is within desired angle OR
                    currentHeuristic > targetHeuristic)                                       //A target has been chosen but current target is more viable
                    {
                        target = potentialTarget;           //Set as chosen target
                        targetHeuristic = currentHeuristic; //Update target heuristic
                    }
                }
            }

            //Cleanup:
            if (target != null && !settings.alwaysLookForTarget) break; //Break out of loop once target is found (unless projectile is always looking for target)
            yield return new WaitForSeconds(secsPerUpdate);             //Wait until next update
        }
        if (target != null) print("Target: " + target.name);
    }

    //RUNTIME METHODS:
    private protected virtual void Awake()
    {
        //Check settings:
        if (settings == null) //Settings were not provided
        {
            Debug.LogWarning("Projectile " + name + " is missing settings, using system defaults.");    //Log warning in case someone forgot
            settings = (ProjectileSettings)Resources.Load("DefaultSettings/DefaultProjectileSettings"); //Load default settings from Resources folder
        }
    }
    private protected virtual void FixedUpdate()
    {
        //Modify velocity:
        if (target != null) //Projectile has a target
        {
            Vector3 currentTargetDirection = (target.position - transform.position).normalized; //Current direction toward target
            Vector3 newForward = Vector3.Lerp(velocity.normalized, currentTargetDirection, settings.targetingStrength);
            velocity = velocity.magnitude * newForward;
        }
        if (settings.drop > 0) velocity.y -= settings.drop * Time.fixedDeltaTime;  //Perform bullet drop (downward acceleration) if relevant
        Vector3 targetPos = transform.position + (velocity * Time.fixedDeltaTime); //Get target projectile position
        float travelDistance = Vector3.Distance(transform.position, targetPos);    //Get distance this projectile is moving this update

        //Check range:
        totalDistance += travelDistance; //Add motion to total distance traveled (NOTE: may briefly end up being greater than actual distance traveled)
        if (settings.range > 0 && totalDistance > settings.range) //Distance about to be traveled exceeds range
        {
            //Backtrack target:
            float backtrackAmt = totalDistance - settings.range;                          //Determine length by which to backtrack projectile
            travelDistance -= backtrackAmt;                                               //Adjust travel distance to account for limited range
            targetPos = Vector3.MoveTowards(targetPos, transform.position, backtrackAmt); //Backtrack target position by given amount (ensuring projectile travels exact range)
            totalDistance = settings.range;                                               //Indicate that projectile has now traveled exactly its full range
        }

        //Check for hit:
        if (Physics.Linecast(transform.position, targetPos, out RaycastHit hitInfo, ~settings.ignoreLayers)) //Do a simple linecast, only ignoring designated layers
        {
            totalDistance -= velocity.magnitude - hitInfo.distance; //Update totalDistance to reflect actual distance traveled at exact point of contact
            HitObject(hitInfo);                                     //Trigger hit procedure
            return;                                                 //Do nothing else
        }

        //Perform move:
        transform.position = targetPos;                                       //Move projectile to target position
        transform.rotation = Quaternion.LookRotation(velocity);               //Rotate projectile to align with current velocity
        if (settings.range > 0 && totalDistance >= settings.range) BurnOut(); //Delayed projectile destruction for end of range (ensures projectile dies after being moved)
    }

    //INPUT METHODS:
    /// <summary>
    /// Call this method to fire this projectile from designated barrel.
    /// </summary>
    /// <param name="barrel">Determines starting position, orientation and velocity of projectile.</param>
    public void Fire(Transform barrel)
    {
        //Initialization:
        Fire(barrel.position, barrel.rotation); //Perform normal firing initialization

        //Check for origin player:
        originPlayer = barrel.GetComponentInParent<PlayerController>();                               //Try to get playercontroller from barrel
        if (originPlayer == null) originPlayer = barrel.GetComponentInParent<NetworkPlayer>().player; //Get player script from network player if necessary
    }
    /// <summary>
    /// Call this method if projectile needs to be fired without an object reference (safe for remote projectiles).
    /// </summary>
    public void Fire(Vector3 startPosition, Quaternion startRotation)
    {
        //Initialize values:
        transform.rotation = startRotation;                      //Rotate to initial orientation
        transform.position = startPosition;                      //Move to initial position
        velocity = transform.forward * settings.initialVelocity; //Give projectile initial velocity (aligned with forward direction of barrel)
        if (settings.barrelGap > 0) //Projectile is spawning slightly ahead of barrel
        {
            //Perform a mini position update:
            Vector3 targetPos = startPosition + (transform.forward * settings.barrelGap);                                                        //Get target starting position (with barrel gap)
            if (Physics.Linecast(transform.position, targetPos, out RaycastHit hitInfo, ~settings.ignoreLayers)) { HitObject(hitInfo); return; } //Check for collisions (just in case)

            //Move projectile to target:
            transform.position = targetPos;                                  //Move projectile to starting position
            if (settings.range <= settings.barrelGap) { BurnOut(); return; } //Burn projectile out in the unlikely event that the barrel gap is greater than its range
            totalDistance += settings.barrelGap;                             //Include distance in total distance traveled
        }

        //Cleanup:
        totalDistance = 0;                                            //Reset traveled distance
        if (photonView.IsMine) StartCoroutine(DoTargetAcquisition()); //Begin doing target acquisition on client projectile
    }

    //REMOTE METHODS:
    [PunRPC]
    public void RPC_Fire(Vector3 startPosition, Quaternion startRotation)
    {
        Fire(startPosition, startRotation);
    }
    [PunRPC]
    public void RPC_AcquireTarget()
    {

    }

    //FUNCTIONALITY METHODS:
    /// <summary>
    /// Called whenever projectile strikes an object.
    /// </summary>
    /// <param name="hitInfo">Data about object struck.</param>
    private protected virtual void HitObject(RaycastHit hitInfo)
    {
        //Look for shootable interface:
        IShootable shootable = hitInfo.collider.GetComponent<IShootable>();                     //Try to get shootable component from hit collider object
        if (shootable == null) shootable = hitInfo.collider.GetComponentInParent<IShootable>(); //Try to get shootable component from hit collider object parent
        if (shootable != null) shootable.IsHit(this);                                           //Indicate to object that it has been shot by this projectile

        //Cleanup:
        print("Hit!"); //TEMP
        Delete();      //Destroy projectile
    }
    /// <summary>
    /// Called if projectile range is exhausted and projectile hasn't hit anything.
    /// </summary>
    private protected virtual void BurnOut()
    {
        Delete();
    }
    private void Delete()
    {
        if (!localOnly && TryGetComponent(out PhotonView photonView))
        {
            if (photonView.IsMine) PhotonNetwork.Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
