using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.CoreUtils;

/// <summary>
/// Classes which inherit from this will be able to be attached to the player via a configurable joint.
/// </summary>
public class PlayerEquipment : MonoBehaviour
{
    //Objects & Components:
    private Transform basePlayerTransform; //Master player object which all player equipment (and XR Origin) is under
    private Transform targetTransform;     //Position and orientation for equipment joint to target (should be parent transform)
    private Rigidbody followerBody;        //Transform for object with mimics position and orientation of target equipment joint
    private Rigidbody playerBody;          //Rigidbody attached to player XROrigin

    private protected Rigidbody rb;  //Rigidbody component attached to this script's gameobject
    private ConfigurableJoint joint; //Physical joint connecting this weapon to the player

    //Settings:
    [Header("Settings:")]
    [SerializeField, Tooltip("Settings defining this equipment's physical joint behavior.")] private EquipmentJointSettings jointSettings;
    [SerializeField, Tooltip("Enables constant joint updates for testing purposes.")]        private bool debugUpdateSettings;

    //Runtime Variables:


    //RUNTIME METHODS:
    private protected virtual void Awake()
    {
        //Validity checks:
        XROrigin origin = GetComponentInParent<XROrigin>();                                                                                                                          //Try to get player XR origin
        if (origin == null) { Debug.LogError("PlayerEquipment " + name + " is not childed to a player and must be destroyed."); Destroy(gameObject); }                               //Call error message and abort if player could not be found
        if (!origin.TryGetComponent(out playerBody)) { Debug.LogError("PlayerEquipment " + name + " could not find player rigidbody and must be destroyed."); Destroy(gameObject); } //Call error message and abort if player rigidbody could not be found
        
        //Initial component get:
        basePlayerTransform = origin.transform.parent; //Get root player transform (above XR Origin)
        targetTransform = transform.parent;            //Use current parent as target transform
        transform.parent = basePlayerTransform;        //Reparent equipment to base player transform

        //Check for settings:
        if (jointSettings == null) //No joint settings were provided
        {
            Debug.LogWarning("PlayerEquipment " + name + " is missing jointSettings, using system defaults."); //Log warning in case someone forgot
            jointSettings = (EquipmentJointSettings)Resources.Load("DefaultEquipmentJointSettings");           //Load default settings from Resources folder
        }

        //Instantiate rigidbody follower:
        Transform followerTransform = new GameObject(name + "Follower").transform; //Instantiate empty gameobject as follower
        followerTransform.parent = basePlayerTransform;                            //Child follower to base player transform
        followerTransform.position = targetTransform.position;                     //Set followBody position to exact position of target
        followerTransform.rotation = targetTransform.rotation;                     //Set followBody orientation to exact orientation of target
        followerBody = followerTransform.gameObject.AddComponent<Rigidbody>();     //Give follower a rigidbody component (and save a reference to it)
        followerBody.isKinematic = true;                                           //Make follower rigidbody kinematic
        followerBody.useGravity = false;                                           //Ensure follower body is not affected by gravity

        //Setup rigidbody:
        if (!TryGetComponent(out rb)) rb = gameObject.AddComponent<Rigidbody>();                                      //Make sure system has a rigidbody
        else { Debug.Log("NOTE: The rigidbody on PlayerEquipment " + name + " may have been modified at runtime."); } //Post a note just in case the following messes with someone's rigidbody
        rb.drag = 0;                                                                                                  //Turn off linear drag
        rb.angularDrag = 0;                                                                                           //Turn off angular drag
        rb.useGravity = false;                                                                                        //Turn off rigidbody gravity
        rb.interpolation = RigidbodyInterpolation.Interpolate;                                                        //Enable interpolation
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;                                         //Enable continuous dynamic collisions

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

        //Disable collisions with player:
        foreach (Collider collider in GetComponentsInChildren<Collider>()) //Iterate through each collider in this equipment
        {
            Physics.IgnoreCollision(playerBody.GetComponent<Collider>(), collider, true); //Make physics ignore collisions between equipment colliders and player
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

    //FUNCTIONALITY METHODS:
    /// <summary>
    /// Updates position of rigidbody follower to match position of target.
    /// </summary>
    private void PerformFollowerUpdate()
    {
        //Calculate follower position:
        Vector3 targetPos = targetTransform.position; //Get base target position for rigidbody follower
        if (jointSettings.velocityCompensation > 0) //Velocity compensation is enabled by settings
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