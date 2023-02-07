using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Pun;

/// <summary>
/// Base class for any non-hitscan ballistic projectiles.
/// </summary>
public class Projectile : MonoBehaviour
{
    //Objects & Components:

    //Settings:
    [Tooltip("Settings object determining base properties of projectile.")] public ProjectileSettings settings;

    //Runtime Variables:
    private Vector3 velocity;              //Speed and direction at which projectile is traveling
    private PlayerController originPlayer; //The player which shot this projectile
    private Transform target;              //Transform which projectile is currently homing toward
    private float totalDistance;           //Total travel distance covered by this projectile

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
        //Initialize variables:
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
        //Check for origin player:
        originPlayer = barrel.GetComponentInParent<PlayerController>();                               //Try to get playercontroller from barrel
        if (originPlayer == null) originPlayer = barrel.GetComponentInParent<NetworkPlayer>().player; //Get player script from network player if necessary

        //Initialize values:
        velocity = barrel.forward * settings.initialVelocity; //Give projectile initial velocity (aligned with forward direction of barrel)
        transform.position = barrel.transform.position;       //Move to initial position
        transform.rotation = barrel.transform.rotation;       //Rotate to initial orientation
        if (settings.barrelGap > 0) //Projectile is spawning slightly ahead of barrel
        {
            //Perform a mini position update:
            Vector3 targetPos = barrel.position + (barrel.forward * settings.barrelGap);                                                         //Get target starting position (with barrel gap)
            if (Physics.Linecast(transform.position, targetPos, out RaycastHit hitInfo, ~settings.ignoreLayers)) { HitObject(hitInfo); return; } //Check for collisions (just in case)

            //Move projectile to target:
            transform.position = targetPos;                                  //Move projectile to starting position
            if (settings.range <= settings.barrelGap) { BurnOut(); return; } //Burn projectile out in the unlikely event that the barrel gap is greater than its range
            totalDistance += settings.barrelGap;                             //Include distance in total distance traveled
        }
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
        if (TryGetComponent(out PhotonView photonView))
        {
            if (photonView.IsMine) PhotonNetwork.Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
