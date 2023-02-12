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
    private Transform target;              //Transform which projectile is currently homing toward
    private float totalDistance;           //Total travel distance covered by this projectile

    internal bool localOnly = false; //Indicates that this projectile does not have equivalents on the network
    private Vector3 prevTargetPos;   //Previous position of target, used for velocity prediction

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
        foreach (Targetable targetable in Targetable.instances) //Iterate through list of targetables
        {
            //Eliminate non-viable targets:
            Vector3 targetSep = targetable.targetPoint.position - transform.position; //Get distance and direction from projectile to target
            float targetDist = targetSep.magnitude;                                   //Distance from projectile to target
            if (targetDist > RemainingRange) continue;                                //Ignore targets which are outside projectile's potential range
            float targetAngle = Vector3.Angle(targetSep, transform.forward);          //Get angle between target direction and projectile movement direction
            if (targetAngle > settings.targetDesignationAngle.y) continue;            //Ignore targets which are behind the projectile and will likely never be hit

            //Cleanup:
            potentialTargets.Add(targetable.targetPoint); //Add valid targets to list of targets to check
        }

        //Look for targets:
        while (potentialTargets.Count > 0) //Run forever (while projectile has targets to consider
        {
            for (int x = 0; x < potentialTargets.Count;) //Iterate through list of potential targets (allow internal functionality to manually index to next target)
            {
                //Eliminate non-viable targets:
                Transform potentialTarget = potentialTargets[x];                             //Get reference to current target
                Vector3 targetSep = potentialTarget.position - transform.position;           //Get distance and direction from projectile to target
                float targetDist = targetSep.magnitude;                                      //Distance from projectile to target
                //if (targetDist > RemainingRange) { potentialTargets.RemoveAt(x); continue; } //Remove target from potentials list if it can never be reached by projectile (still tracks it if it's the current target and a more viable target has not yet been chosen)
                float targetAngle = Vector3.Angle(targetSep, transform.forward);             //Get angle between target direction and projectile movement direction
                if (targetAngle > settings.targetDesignationAngle.y)                         //Angle to potential target is so steep that projectile will likely never hit
                {
                    potentialTargets.RemoveAt(x); //Remove this target from list of potential targets
                    if (potentialTarget == target) //Active target has just been passed
                    {
                        target = null;       //Clear active target
                        targetHeuristic = 0; //Clear current target heuristic
                    }
                    continue; //Skip to next potential target
                }

                //Check for viable targets:
                if (targetDist <= settings.targetingDistance && targetAngle <= settings.targetDesignationAngle.x) //Current targetable is within range and within acquisition angle
                {
                    //Check for obstructions:
                    if (settings.LOSTargeting) //System is using line-of-sight targeting
                    {
                        if (Physics.Linecast(transform.position, potentialTarget.position, settings.targetingIgnoreLayers)) //Object is obstructed
                        {
                            if (potentialTarget == target) //Active target has been obstructed
                            {
                                target = null;       //Clear active target
                                targetHeuristic = 0; //Clear current target heuristic
                            }
                            x++; continue; //Move to next potential target
                        }
                    }

                    //Check target viability:
                    float currentHeuristic = (1 - settings.angleDistancePreference) * Mathf.InverseLerp(settings.targetDesignationAngle.x, 0, targetAngle); //Calculate angle preference score for this target
                    currentHeuristic += settings.angleDistancePreference * Mathf.InverseLerp(settings.targetingDistance, 0, targetDist);                    //Calculate proximity preference score for this target

                    //Update current target:
                    if (target == potentialTarget) targetHeuristic = currentHeuristic; //Update current target heuristic whenever current target is re-acquired
                    else if (target == null || currentHeuristic > targetHeuristic) //Either projectile has no current target or potential target is more viable than current target
                    {
                        target = potentialTarget;           //Set potential target as chosen target
                        prevTargetPos = target.position;    //Initialize target position tracker
                        targetHeuristic = currentHeuristic; //Update target heuristic
                    }
                }

                //Cleanup:
                x++; //Move to next potential target
            }

            //Cleanup:
            if (target != null && !settings.alwaysLookForTarget) break; //Break out of loop once target is found (unless projectile is always looking for target)
            yield return new WaitForSeconds(secsPerUpdate);             //Wait until next update
        }
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
    private protected virtual void Update()
    {
        if (photonView.IsMine || localOnly) //Only truly compute positional updates for master projectile
        {
            //Modify velocity:
            if (settings.drop > 0) velocity.y -= settings.drop * Time.deltaTime;  //Perform bullet drop (downward acceleration) if relevant
            if (target != null) //Projectile has a target
            {
                //Get effective target position:
                Vector3 targetPos = target.position; //Initialize target position marker
                if (settings.predictionIterations > 0) //Projectile is using velocity prediction
                {
                    Vector3 targetVelocity = (targetPos - prevTargetPos) / Time.deltaTime; //Approximate velocity of target object
                    float currentSpeed = velocity.magnitude;                               //Get current speed here so it only has to be calculated once
                    for (int x = 0; x < settings.predictionIterations; x++) //Repeat for set number of iterations
                    {
                        float distanceToTarget = Vector3.Distance(transform.position, targetPos); //Get distance between projectile and target
                        float timeToTarget = distanceToTarget / currentSpeed;                     //Approximate time it would take to reach target
                        targetPos = target.position + (targetVelocity * timeToTarget);            //Adjust effective target position to where target will be when reached
                    }
                    prevTargetPos = target.position; //Update target position tracker
                }

                //Curve to meet target:
                Vector3 newVelocity = (targetPos - transform.position).normalized * velocity.magnitude;          //Get velocity which would point projectile directly at target
                velocity = Vector3.MoveTowards(velocity, newVelocity, settings.homingStrength * Time.deltaTime); //Incrementally adjust toward target velocity
            }

            //Get target position:
            Vector3 newPosition = transform.position + (velocity * Time.deltaTime);   //Get target projectile position
            float travelDistance = Vector3.Distance(transform.position, newPosition); //Get distance this projectile is moving this update

            //Check range:
            totalDistance += travelDistance; //Add motion to total distance traveled (NOTE: may briefly end up being greater than actual distance traveled)
            if (settings.range > 0 && totalDistance > settings.range) //Distance about to be traveled exceeds range
            {
                //Backtrack target:
                float backtrackAmt = totalDistance - settings.range;                              //Determine length by which to backtrack projectile
                travelDistance -= backtrackAmt;                                                   //Adjust travel distance to account for limited range
                newPosition = Vector3.MoveTowards(newPosition, transform.position, backtrackAmt); //Backtrack target position by given amount (ensuring projectile travels exact range)
                totalDistance = settings.range;                                                   //Indicate that projectile has now traveled exactly its full range
            }

            //Check for hit:
            if (Physics.Linecast(transform.position, newPosition, out RaycastHit hitInfo, ~settings.ignoreLayers)) //Do a simple linecast, only ignoring designated layers
            {
                totalDistance -= velocity.magnitude - hitInfo.distance; //Update totalDistance to reflect actual distance traveled at exact point of contact
                HitObject(hitInfo);                                     //Trigger hit procedure
                return;                                                 //Do nothing else
            }

            //Perform move:
            transform.position = newPosition;                                                          //Move projectile to target position
            transform.rotation = Quaternion.LookRotation(velocity);                                    //Rotate projectile to align with current velocity
            //if (!localOnly) photonView.RPC("RPC_Move", RpcTarget.Others, newPosition, Time.deltaTime); //Move all projectiles on network (unless projectile is local only)
            if (settings.range > 0 && totalDistance >= settings.range) BurnOut();                      //Delayed projectile destruction for end of range (ensures projectile dies after being moved)
        }
        else
        {
            transform.position += velocity * Time.deltaTime;
        }
    }

    //INPUT METHODS:
    /// <summary>
    /// Call this method to fire this projectile from designated barrel.
    /// </summary>
    /// <param name="barrel">Determines starting position, orientation and velocity of projectile.</param>
    public void Fire(Transform barrel)
    {
        Fire(barrel.position, barrel.rotation); //Break transform apart and perform normal firing operation
    }
    /// <summary>
    /// Call this method if projectile needs to be fired without an object reference (safe for remote projectiles).
    /// </summary>
    public void Fire(Vector3 startPosition, Quaternion startRotation)
    {
        //Initialize values:
        Vector3 targetPosition = startPosition;                  //Initialize value for ideal starting position
        transform.rotation = startRotation;                      //Rotate to initial orientation
        velocity = transform.forward * settings.initialVelocity; //Give projectile initial velocity (aligned with forward direction of barrel)

        //Check barrel gap:
        if (settings.barrelGap > 0) //Projectile is spawning slightly ahead of barrel
        {
            //Perform a mini position update:
            targetPosition += transform.forward * settings.barrelGap;                                                                            //Get target starting position (with barrel gap)
            if (Physics.Linecast(startPosition, targetPosition, out RaycastHit hitInfo, ~settings.ignoreLayers)) { HitObject(hitInfo); return; } //Check for collisions (just in case)
            if (settings.range <= settings.barrelGap) { BurnOut(); return; } //Burn projectile out in the unlikely event that the barrel gap is greater than its range
            totalDistance += settings.barrelGap;                             //Include distance in total distance traveled
        }

        //Cleanup:
        transform.position = targetPosition;                                             //Move to initial position
        //if (!localOnly) photonView.RPC("RPC_Move", RpcTarget.Others, targetPosition, 1); //Move all networked projectiles to starting position
        if (settings.homingStrength > 0) StartCoroutine(DoTargetAcquisition());          //Begin doing target acquisition
    }

    //REMOTE METHODS:
    /// <summary>
    /// Moves projectile to target position (used to move remote projectiles).
    /// </summary>
    [PunRPC]
    public void RPC_Move(Vector3 newPosition, float deltaTime)
    {
        velocity = (newPosition - transform.position) / deltaTime;                            //Get current projectile velocity
        transform.position = newPosition;                                                     //Move to new position
        if (velocity != Vector3.zero) transform.rotation = Quaternion.LookRotation(velocity); //Rotate projectile to face direction of travel
    }

    //FUNCTIONALITY METHODS:
    /// <summary>
    /// Called whenever projectile strikes an object.
    /// </summary>
    /// <param name="hitInfo">Data about object struck.</param>
    private protected virtual void HitObject(RaycastHit hitInfo)
    {
        //Look for strikeable scripts:
        NetworkPlayer targetPlayer = hitInfo.collider.GetComponentInParent<NetworkPlayer>();              //Try to get network player from hit collider
        if (targetPlayer != null) targetPlayer.photonView.RPC("RPC_Hit", RpcTarget.All, settings.damage); //Indicate to player that it has been hit
        else //Hit object is not a player
        {
            Targetable targetObject = hitInfo.collider.GetComponent<Targetable>(); //Try to get targetable script from target
            if (targetObject != null) targetObject.IsHit(settings.damage);         //Indicate to targetable that it has been hit
        }

        //Cleanup:
        Delete(); //Destroy projectile
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
