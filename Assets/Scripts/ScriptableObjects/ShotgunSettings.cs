using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Determines properties of player shotgun.
/// </summary>
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/ShotgunSettings", order = 1)]
public class ShotgunSettings : ScriptableObject
{
    [Header("Mechanical Properties:")]
    [Tooltip("Name of projectile prefab fired by this weapon (make sure this refers to a projectile in the Resources/Projectiles folder).")] public string projectileResourceName;
    [Min(1), Tooltip("Maximum number of shots which can be loaded into weapon (not necessarily equal to number of barrels).")]               public int maxLoadedShots = 2;
    [Range(0, 90), Tooltip("Angle which barrels snap to when breach is open.")]                                                              public float breakAngle = 45;
    [Min(0), Tooltip("How much time weapon needs to be left open for in order for it to be reloaded.")]                                      public float cooldownTime = 0.7f;
    [Header("Locomotion:")]
    [Min(0), Tooltip("Magnitude of velocity imparted on player when weapon is fired (primary locomotion setting).")] public float fireVelocity;
    [Header("Gunfeel:")]
    [Range(0, 1), Tooltip("How far back the player has to pull the trigger before it fires.")]             public float triggerThreshold = 1;
    [Tooltip("Amount of upward recoil force applied to weapon rigidbody when firing (mostly aesthetic).")] public float recoilTorque;
    [Header("Sounds:")]
    [Tooltip("SFX for when weapon is fired")]  public AudioClip fireSound;
    [Tooltip("SFX for when breach is opened")] public AudioClip ejectSound;
    [Tooltip("SFX for when breach is closed")] public AudioClip lockSound;
}
