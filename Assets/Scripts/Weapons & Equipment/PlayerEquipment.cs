using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.CoreUtils;
using CustomEnums;
using UnityEngine.InputSystem;

/// <summary>
/// Classes which inherit from this will be able to be attached to the player via a configurable joint.
/// </summary>
public class PlayerEquipment : MonoBehaviour
{
    //Objects & Components:
    internal PlayerController player;       //Player currently controlling this equipment
    private Transform basePlayerTransform;  //Master player object which all player equipment (and XR Origin) is under
    private protected Transform targetTransform;      //Position and orientation for equipment joint to target (should be parent transform)
    private Rigidbody followerBody;         //Transform for object with mimics position and orientation of target equipment joint
    private protected Rigidbody playerBody; //Rigidbody attached to player XROrigin
    private InputActionMap inputMap;        //Input map which this equipment will use

    private protected Rigidbody rb;            //Rigidbody component attached to this script's gameobject
    private protected AudioSource audioSource; //Audio source component for playing sounds made by this equipment
    private ConfigurableJoint joint;           //Physical joint connecting this weapon to the player

    //Settings:
    [Header("Settings:")]
    [SerializeField, Tooltip("Settings defining this equipment's physical joint behavior.")] private EquipmentJointSettings jointSettings;
    [SerializeField, Tooltip("Enables constant joint updates for testing purposes.")] private protected bool debugUpdateSettings;

    //Runtime Variables:
    /// <summary>
    /// Which hand this equipment is associated with (if any).
    /// </summary>
    internal Handedness handedness = Handedness.None;
    /// <summary>
    /// Equipment in stasis will do nothing and check nothing until it is re-equipped to a player.
    /// </summary>
    internal bool inStasis = false;

    //RUNTIME METHODS:
    private protected virtual void Awake()
    {
        //Flexible system setup:
        player = GetComponentInParent<PlayerController>(); //Try to get player controlling this equipment
        if (player != null) //Equipment is childed to a player (normal operation)
        {
            //Validity checks:
            PlayerInput playerInput = player.GetComponent<PlayerInput>(); if (playerInput == null) { Debug.LogError("PlayerEquipment " + name + " could not find a PlayerInput on Player"); Destroy(gameObject); } //Make sure equipment can find player input component
            XROrigin origin = GetComponentInParent<XROrigin>();                                                                                                                                                    //Try to get player XR origin
            if (origin == null) { Debug.LogError("PlayerEquipment " + name + " is not childed to an XR Origin and must be destroyed."); Destroy(gameObject); }                                                     //Call error message and abort if player could not be found
            if (!origin.TryGetComponent(out playerBody)) { Debug.LogError("PlayerEquipment " + name + " could not find player rigidbody and must be destroyed."); Destroy(gameObject); }                           //Call error message and abort if player rigidbody could not be found
            player.attachedEquipment.Add(this);                                                                                                                                                                    //Indicate that this equipment has now been attached to player

            //Initial component setup:
            basePlayerTransform = origin.transform.parent; //Get root player transform (above XR Origin)
            targetTransform = transform.parent;            //Use current parent as target transform
            transform.parent = basePlayerTransform;        //Reparent equipment to base player transform
            InitializeFollower(basePlayerTransform);       //Initialize rigidbody follower system

            //Check handedness & setup input:
            if (targetTransform.name.Contains("Left") || targetTransform.name.Contains("left")) //Equipment is being attached to the left hand/side
            {
                inputMap = playerInput.actions.FindActionMap("XRI LeftHand Interaction"); //Get left hand input map
                handedness = Handedness.Left;                                             //Indicate left-handedness
            }
            else if (targetTransform.name.Contains("Right") || targetTransform.name.Contains("right")) //Equipment is being attached to the right hand/side
            {
                inputMap = playerInput.actions.FindActionMap("XRI RightHand Interaction"); //Get right hand input map
                handedness = Handedness.Right;                                             //Indicate right-handedness
            }
            else //Equipment is not being attached to an identifiable side
            {
                inputMap = playerInput.actions.FindActionMap("XRI Generic Interaction"); //Get generic input map
                handedness = Handedness.None;                                            //Indicate that equipment is not attached to a side
            }
            if (inputMap != null) inputMap.actionTriggered += TryGiveInput; //Otherwise, subscribe to input triggered event
            else Debug.LogWarning("PlayerEquipment " + name + " could not get its desired input map, make sure PlayerInput's actions are set up properly."); //Post warning if input get was unsuccessful
        }
        else //Equipment is not being controlled by a player (probably for demo purposes
        {
            //Initial component setup:
            if (transform.parent.TryGetComponent(out DemoEquipmentMount mount)) { mount.equipment = this; } //Send reference to equipment mount if relevant
            targetTransform = transform.parent;                                                             //Make parent the effective target handle
            transform.parent = null;                                                                        //Unchild weapon from parent
            InitializeFollower(null);                                                                       //Initialize rigidbody follower system, unchilding system from any parent
        }

        //Universal component setup:
        if (!TryGetComponent(out audioSource)) audioSource = gameObject.AddComponent<AudioSource>(); //Make sure equipment has audio source

        //Setup rigidbody:
        if (!TryGetComponent(out rb)) rb = gameObject.AddComponent<Rigidbody>();                                      //Make sure system has a rigidbody
        else { Debug.Log("NOTE: The rigidbody on PlayerEquipment " + name + " may have been modified at runtime."); } //Post a note just in case the following messes with someone's rigidbody
        rb.drag = 0;                                                                                                  //Turn off linear drag
        rb.angularDrag = 0;                                                                                           //Turn off angular drag
        rb.useGravity = false;                                                                                        //Turn off rigidbody gravity
        rb.isKinematic = false;                                                                                       //Make sure rigidbody is not kinematic
        rb.interpolation = RigidbodyInterpolation.Interpolate;                                                        //Enable interpolation
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;                                         //Enable continuous dynamic collisions

        //Check for settings:
        if (jointSettings == null) //No joint settings were provided
        {
            Debug.Log("PlayerEquipment " + name + " is missing jointSettings, using system defaults.");              //Log warning in case someone forgot
            jointSettings = (EquipmentJointSettings)Resources.Load("DefaultSettings/DefaultEquipmentJointSettings"); //Load default settings from Resources folder
        }

        //Setup configurable joint:
        joint = gameObject.AddComponent<ConfigurableJoint>();   //Instantiate a configurable joint on this equipment gameobject
        joint.connectedBody = followerBody;                     //Connect joint to follower transform
        joint.xMotion = ConfigurableJointMotion.Limited;        //Enable X axis linear motion limits
        joint.yMotion = ConfigurableJointMotion.Limited;        //Enable Y axis linear motion limits
        joint.zMotion = ConfigurableJointMotion.Limited;        //Enable Z axis linear motion limits
        joint.angularXMotion = ConfigurableJointMotion.Limited; //Enable X axis angular motion limits
        joint.angularYMotion = ConfigurableJointMotion.Limited; //Enable Y axis angular motion limits
        joint.angularZMotion = ConfigurableJointMotion.Limited; //Enable Z axis angular motion limits
        ConfigureJoint();                                       //Perform the remainder of joint configuration in a separate function

        //Ignore player collisions:
        if (playerBody != null) //System is attached to a player with a rigidbody
        {
            foreach (Collider collider in GetComponentsInChildren<Collider>()) //Iterate through each collider in this equipment
            {
                Physics.IgnoreCollision(playerBody.GetComponent<Collider>(), collider, true); //Make physics ignore collisions between equipment colliders and player
            }
        }
    }
    private protected virtual void Update()
    {
        if (debugUpdateSettings && Application.isEditor) ConfigureJoint(); //Reconfigure joint every update if debug setting is selected (only necessary in Unity Editor)
    }
    private protected virtual void FixedUpdate()
    {
        PerformFollowerUpdate(); //Update follower transform
    }
    private protected virtual void OnPreRender()
    {
        PerformFollowerUpdate(); //Update follower transform
    }
    private protected virtual void OnDestroy()
    {
        //Unsubscribe from events:
        if (inputMap != null) inputMap.actionTriggered -= TryGiveInput; //Unsubscribe from input event
    }
    private void TryGiveInput(InputAction.CallbackContext context) { if (!player.InCombat()) InputActionTriggered(context); }
    private protected virtual void InputActionTriggered(InputAction.CallbackContext context) { }

    //FUNCTIONALITY METHODS:
    /// <summary>
    /// Equips equipment onto target transform (should be under a player's XR Origin).
    /// </summary>
    /// <param name="target">Needs to be under a specific player's XR Origin, named "Left..." or "Right..." if it's on a specific side of the player.</param>
    public void Equip(Transform target)
    {

    }
    /// <summary>
    /// Detaches equipment from player and puts it into stasis.
    /// </summary>
    public void UnEquip()
    {

    }
    /// <summary>
    /// Updates position of rigidbody follower to match position of target.
    /// </summary>
    private void PerformFollowerUpdate()
    {
        //Calculate follower position:
        Vector3 targetPos = targetTransform.position; //Get base target position for rigidbody follower
        if (jointSettings.velocityCompensation > 0 && playerBody != null) //Velocity compensation is enabled by settings (and has a playerBody to reference)
        {
            targetPos += playerBody.velocity * jointSettings.velocityCompensation; //Adjust target position based on velocity compensation to account for lag
        }
        if (jointSettings.offset != Vector3.zero) //Target offset mode is enabled by settings
        {
            targetPos += transform.rotation * jointSettings.offset; //Apply offset to target position (orient offset to current object orientation)
        }

        //Apply follower transforms:
        followerBody.MovePosition(targetPos);                //Apply target position through follower rigidbody
        followerBody.MoveRotation(targetTransform.rotation); //Apply target rotation through follower rigidbody
    }

    //UTILITY METHODS:
    /// <summary>
    /// Initializes rigidbody follower system as child of target parent.
    /// </summary>
    private void InitializeFollower(Transform followerParent)
    {
        Transform followerTransform = new GameObject(name + "Follower").transform; //Instantiate empty gameobject as follower
        followerTransform.parent = followerParent;                                 //Child follower to target parent
        followerTransform.position = targetTransform.position;                     //Set followBody position to exact position of target
        followerTransform.rotation = targetTransform.rotation;                     //Set followBody orientation to exact orientation of target
        followerBody = followerTransform.gameObject.AddComponent<Rigidbody>();     //Give follower a rigidbody component (and save a reference to it)
        followerBody.isKinematic = true;                                           //Make follower rigidbody kinematic
        followerBody.useGravity = false;                                           //Ensure follower body is not affected by gravity
    }
    /// <summary>
    /// Applies current joint settings to local ConfigurableJoint.
    /// </summary>
    private void ConfigureJoint()
    {
        //Validity checks:
        if (joint == null) { Debug.LogWarning("PlayerEquipment " + name + " tried to update joint before joint was instantiated."); return; } //Log warning and abort if there is no joint to update
        if (jointSettings == null) { Debug.LogWarning("PlayerEquipment " + name + " tried to update joint without jointSettings."); return; } //Log warning and abort if there are no joint settings to reference

        //Apply limit springs:
        SoftJointLimitSpring spring = new SoftJointLimitSpring(); //Initialize variable for setting spring values
        spring.spring = jointSettings.angularSpringiness;         //Set angular X limit springiness
        spring.damper = jointSettings.angularDampening;           //Set angular X limit dampening
        joint.angularXLimitSpring = spring;                       //Apply changes to angular X spring
        //spring.damper = 0; //Try uncommenting this if joint feels weird
        joint.angularYZLimitSpring = spring;                      //Apply changes to anglular YZ spring

        //Apply limits:
        SoftJointLimit limit = new SoftJointLimit();       //Initialize variable for setting limit values
        limit.limit = (jointSettings.limitAngle / 2) * -1; //Set lower angular X limit (to half of full setting because it is split between two separate limits)
        limit.bounciness = jointSettings.limitBounciness;  //Set lower angular X limit bounciness
        joint.lowAngularXLimit = limit;                    //Apply changes to low angular X limit
        limit.limit *= -1;                                 //Set upper angular X limit (positive half of lower limit)
        //limit.bounciness = 0; //Try uncommenting this if joint feels weird
        joint.highAngularXLimit = limit;                   //Apply changes to high angular X limit
        limit.limit *= 2;                                  //Set angular Y and Z limits (double to get back to full setting)
        //limit.bounciness = jointSettings.limitBounciness; //Uncomment this if you uncommented the previous commented line
        joint.angularYLimit = limit;                       //Apply changes to angular Y limit
        joint.angularZLimit = limit;                       //Apply changes to angular Z limit

        //Apply drives:
        JointDrive drive = new JointDrive();                     //Initialize variable for setting drive values
        drive.positionSpring = jointSettings.linearDrive;        //Set linear drive spring force
        drive.maximumForce = joint.xDrive.maximumForce;          //Get maximum drive from configurableJoint default setting
        joint.xDrive = drive;                                    //Apply setting to linear X drive
        joint.yDrive = drive;                                    //Apply setting to linear Y drive
        joint.zDrive = drive;                                    //Apply setting to linear Z drive
        drive.positionSpring = jointSettings.angularDrive;       //Set angular drive spring force
        drive.positionDamper = jointSettings.angularDriveDamper; //Set angular drive dampening effect
        joint.angularXDrive = drive;                             //Apply setting to angular X drive
        joint.angularYZDrive = drive;                            //Apply setting to angular YZ drive
    }
}