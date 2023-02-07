using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// New controller for player shotgun, fires a single slug.
/// </summary>
public class NewShotgunController : PlayerEquipment
{
    //Objects & Components:
    private ConfigurableJoint breakJoint; //Joint controlling weapon's break action

    //Settings:
    [Header("Settings:")]
    [SerializeField, Tooltip("Settings object which determines general weapon behavior.")] private ShotgunSettings gunSettings;

    //Runtime Variables:


    //RUNTIME METHODS:
    private protected override void Awake()
    {
        //Initialization:
        base.Awake(); //Run base awake method

        //Check settings:
        if (gunSettings == null) //No weapon settings were provided
        {
            Debug.Log("Weapon " + name + " is missing Gun Settings, using system defaults.");        //Log warning in case someone forgot
            gunSettings = (ShotgunSettings)Resources.Load("DefaultSettings/DefaultShotgunSettings"); //Use default settings from Resources
        }

        //Get objects & components:

    }

    //INPUT METHODS:
    /// <summary>
    /// Shoots the gun (instantiates projectiles in network if possible).
    /// </summary>
    public void Fire()
    {

    }

    //FUNCTIONALITY METHODS:

}
