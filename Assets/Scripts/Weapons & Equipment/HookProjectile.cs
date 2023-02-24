using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

/// <summary>
/// Special kind of projectile used for grappling hook system.
/// </summary>
public class HookProjectile : Projectile
{
    //Classes, Enums & Structs:
    /// <summary>
    /// Indicates certain distinct types of hook projectile behavior.
    /// </summary>
    public enum HookState
    {
        /// <summary>Hook is hidden and ready to be fired.</summary>
        Stowed,
        /// <summary>Hook has been launched and is freely traveling through the air.</summary>
        Deployed,
        /// <summary>Hook is not locked on to anything and is retracting back toward player's arm.</summary>
        Retracting,
        /// <summary>Hook is locked into a stationary object (such as a wall) and is pulling the player forward.</summary>
        Hooked,
        /// <summary>Hook is locked into another player and shenanigans are about to ensue.</summary>
        PlayerTethered
    }

    //Objects & Components:
    internal NewGrapplerController controller; //Weapon which controls this hook
    private Transform lockPoint;               //Physical point on projectile which locks onto hit objects. Hook is rotated around this point while it is in pull mode.
    private Transform tetherPoint;             //Physical point on hook which tether line is attached to
    private LineRenderer tether;               //Component which draws the line between projectile and launcher

    //Runtime Variables:
    /// <summary>
    /// Current behavior state of this hook projectile.
    /// </summary>
    internal HookState state = HookState.Stowed;
    internal NetworkPlayer hitPlayer;  //The player this hook is currently locked into (usually null)
    private Vector3 hitPosition;       //Position this hook is currently locked into (usually irrelevant)
    private LayerMask lineCheckLayers; //Layermask for checking intersections with the line (does not count for players)
    
    internal float timeInState; //Indicates how long it has been since hook state has last changed (always counting up)
    private float retractSpeed; //Current speed (in meters per second) at which hook is being retracted

    //EVENTS:

    //RUNTIME METHODS:
    private protected override void Awake()
    {
        //Initialization:
        base.Awake(); //Call base awake method

        //Get objects & components:
        lockPoint = transform.Find("LockPoint"); if (lockPoint == null) lockPoint = transform;                                               //Get lock point transform (use own transform if not given)
        tetherPoint = transform.Find("TetherPoint"); if (tetherPoint == null) tetherPoint = transform;                                       //Get tether point transform (use own transform if not given)
        tether = GetComponent<LineRenderer>(); if (tether == null) { Debug.LogWarning("Grappling Hook projectile needs a line renderer."); } //Get line renderer component

        //Initialize runtime vars:
        lineCheckLayers = settings.ignoreLayers |= (1 << LayerMask.NameToLayer("Player")); //Add player to ignore layers to get layers ignored by line check (preventing self-collision)
    }
    private protected override void Update()
    {
        if (state == HookState.Deployed) base.Update(); //Do base projectile homing debug while hook is deployed and homing

        //Update tether:
        if (state != HookState.Stowed) //Hook is currently out and about
        {
            if (photonView.IsMine) //Local tether update
            {
                tether.SetPosition(0, controller.barrel.position); //Move start of line to current position of launcher (on player arm)
                tether.SetPosition(1, tetherPoint.position);       //Move end of line to current position of this projectile
            }
            else //Remote tether update
            {

            }
        }

        //Update timers:
        if (photonView.IsMine) //Only update master version's timers
        {
            timeInState += Time.deltaTime; //Increment timer tracking time spent in current behavior state
            if (state == HookState.Retracting) //Hook is currently being retracted
            {
                retractSpeed += controller.settings.retractAcceleration * Time.deltaTime; //Add acceleration to retraction speed
            }
        }
    }
    private protected override void FixedUpdate()
    {
        //Determine hook behavior:
        if (!photonView.IsMine) return; //Don't let networked versions do any movement (leave it all up to the transformView)
        switch (state) //Determine update behavior of projectile depending on which state it is in
        {
            case HookState.Deployed: //Hook has been launched and is flying through the air
                base.FixedUpdate(); //Use normal projectile movement and homing
                if (controller.settings.intersectBehavior != HookshotSettings.LineIntersectBehavior.Ignore) //Hook needs to check if anything is intersecting its line
                {
                    if (Physics.Linecast(controller.barrel.position, transform.position, out RaycastHit hitInfo, lineCheckLayers)) //Check along line for collisions
                    {
                        if (HitsOwnPlayer(hitInfo)) { Debug.LogWarning("Grappling hook line just tried to hit player for some reason, check lineCheckLayers."); break; } //Super make sure line can't hit own player
                        if (controller.settings.intersectBehavior == HookshotSettings.LineIntersectBehavior.Release) { Release(); break; }                               //Behavior is set to release on line intersection
                        if (controller.settings.intersectBehavior == HookshotSettings.LineIntersectBehavior.Grab) { HitObject(hitInfo); break; }                         //Behavior is set to grab on line intersection
                    }
                }
                break;
            case HookState.Retracting: //Hook is released and is being pulled back toward player
                //Point away from player:
                Vector3 pointDirection = (controller.barrel.position - transform.position).normalized; //Get direction from projectile to player
                transform.forward = -pointDirection;                                                   //Always point grappling hook in direction of player

                //Move toward player:
                transform.position = Vector3.MoveTowards(transform.position, controller.barrel.position, retractSpeed * Time.fixedDeltaTime); //Retract toward player at given speed
                if (transform.position == controller.barrel.position) Stow();                                                                 //Stow once hook has gotten close enough to destination
                break;
            case HookState.Hooked: //Grappling hook is attached to a stationary object
                PointLock(hitPosition); //Rotate hook toward controlling player, maintaining world position of lock point
                break;
            case HookState.PlayerTethered: //Grappling hook is attached to an enemy player
                PointLock(hitPlayer.GetComponent<Targetable>().targetPoint.position); //Rotate hook toward controlling player, maintaining position at center mass of tethered player
                break;
            default: break;
        }
    }

    //FUNCTIONALITY METHODS:
    /// <summary>
    /// Un-stows and fires grappling hook (mostly like a normal projectile).
    /// </summary>
    public override void Fire(Vector3 startPosition, Quaternion startRotation, int playerID)
    {
        //Deployment:
        transform.parent = null;                           //Un-child from stow point
        transform.localScale = Vector3.one;                //Make sure scale is right after unchilding
        base.Fire(startPosition, startRotation, playerID); //Use base fire method to physically launch hook
        estimatedLifeTime = -1;                            //Do not use lifetime system for this type of projectile

        //Initialize tether:
        tether.SetPosition(0, controller.barrel.position); //Move start of line to current position of launcher (on player arm)
        tether.SetPosition(1, tetherPoint.position);       //Move end of line to current position of this projectile
        tether.enabled = true;                             //Make tether visible

        //Cleanup:
        ChangeVisibility(true);     //Make projectile visible
        state = HookState.Deployed; //Indicate that hook is now deployed
        timeInState = 0;            //Reset state timer
    }
    public override void Fire(Transform barrel, int playerID)
    {
        Fire(barrel.position, barrel.rotation, playerID);
    }
    /// <summary>
    /// Hides and stows this projectile on a designated position on player hand.
    /// </summary>
    public void Stow()
    {
        //Move to position:
        transform.parent = controller.stowPoint;   //Child hook to stow point
        transform.localPosition = Vector3.zero;    //Zero out position relative to stow point
        transform.localEulerAngles = Vector3.zero; //Zero out rotation relative to stow point

        //Cleanup:
        tether.enabled = false;   //Hide tether
        ChangeVisibility(false);  //Immediately make projectile invisible
        state = HookState.Stowed; //Indicate that projectile is stowed
        timeInState = 0;          //Reset state timer
        retractSpeed = 0;         //Reset retraction speed
    }
    /// <summary>
    /// Version of stow method meant to be the first thing called on new hook projectile by its controller. Passes along the reference so everything runs smoothly.
    /// </summary>
    public void Stow(NewGrapplerController newController)
    {
        controller = newController; //Store controller reference
        Stow();                     //Call base method
    }
    /// <summary>
    /// Causes hook to let go of whatever it is attached to and begin retracting back toward the player.
    /// </summary>
    public void Release(bool bounce = false)
    {
        //Begin retraction:
        retractSpeed = controller.settings.baseRetractSpeed; //Get retraction speed from controller settings

        //Effects:
        if (bounce) controller.Bounced(); //Indicate to controller that hook has bounced off of something
        else controller.ForceReleased();  //Indicate to controller that hook has been released

        //Cleanup:
        transform.localScale = Vector3.one; //Make sure projectile is at its base scale
        hitPlayer = null;                   //Clear any references to tethered players
        state = HookState.Retracting;       //Indicate that hook is now returning to its owner
        timeInState = 0;                    //Reset state timer
        
    }
    /// <summary>
    /// Hook has hit something.
    /// </summary>
    private protected override void HitObject(RaycastHit hitInfo)
    {
        //Check for bounce:
        if (controller.settings.bounceLayers == (controller.settings.bounceLayers | (1 << hitInfo.collider.gameObject.layer))) //Hook is bouncing off of a non-hookable layer
        {
            Release(true); //Release hook immediately
            return;        //Hit resolution has finished
        }
        
        //Check for player:
        hitPlayer = hitInfo.collider.GetComponentInParent<NetworkPlayer>();                //Try to get network player from hit collider
        if (hitPlayer == null) hitPlayer = hitInfo.collider.GetComponent<NetworkPlayer>(); //Try again for network player if it was not initially gotten
        if (hitPlayer != null) //Hit object was a player
        {
            //Validity checks:
            if (hitPlayer.photonView.ViewID == PlayerController.photonView.ViewID) { print("Grappling hook hit own player, despite it all."); Release(); return; } //Prevent hook from ever hitting its own player
            
            //Move to target:
            Transform targetPoint = hitPlayer.GetComponent<Targetable>().targetPoint; //Get target point from hit player (should be approximately center mass)
            transform.parent = targetPoint;                                           //Child hook to player target point
            PointLock(targetPoint.position);                                          //Lock hook to position on target

            //Cleanup
            print("Hooked player!");
            controller.HookedPlayer();        //Indicate to controller that a player has been successfully hooked
            state = HookState.PlayerTethered; //Indicate that a player has been successfully tethered
            timeInState = 0;                  //Reset state timer
            return;                           //Hit resolution has finished
        }

        //Normal hit:
        hitPosition = hitInfo.point; //Mark position hook hit as location to lock at
        PointLock(hitPosition);      //Lock hook to point
        controller.HookedObstacle(); //Indicate to controller that an obstacle has been successfully hooked
        state = HookState.Hooked;    //Indicate that hook is now latched onto a surface
        timeInState = 0;             //Reset state timer
    }
    /// <summary>
    /// Hooks are never destroyed and begin retracting when they run out of range (instead of burning out)
    /// </summary>
    private protected override void BurnOut()
    {
        Release(); //Just have hook release itself when it runs out of range.
    }

    //REMOTE METHODS:

    //UTILITY METHODS:
    /// <summary>
    /// Hides or shows all renderers.
    /// </summary>
    private void ChangeVisibility(bool enable)
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>()) r.enabled = enable; //Enable or disable all renderers
    }
    /// <summary>
    /// Places lockPoint transform position at given world coordinates, and rotates the rest of the hook to point back toward tethered player.
    /// </summary>
    private void PointLock(Vector3 lockPosition)
    {
        Vector3 pointDirection = (controller.barrel.position - lockPosition).normalized;    //Get direction from lock point to end of player grappling arm
        transform.forward = -pointDirection;                                                //Always point grappling hook in direction of player
        transform.position = lockPosition - (transform.rotation * lockPoint.localPosition); //Move hook so that lockPoint is at target position
    }
}
