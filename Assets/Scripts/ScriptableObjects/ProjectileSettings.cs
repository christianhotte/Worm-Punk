using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Determines base properties of a ballistic projectile.
/// </summary>
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/ProjectileSettings", order = 1)]
public class ProjectileSettings : ScriptableObject
{
    [Header("Hit Effect:")]
    [Min(0), Tooltip("How much damage this projectile deals to targets it hits.")] public int damage = 1;
    [Header("Travel Properties:")]
    [Tooltip("Speed at which projectile travels upon spawn.")]                                     public float initialVelocity;
    [Tooltip("Maximum distance projectile can travel (leave zero to make infinite).")]             public float range = 0;
    [Min(0), Tooltip("Distance in front of barrel position at which projectile actually spawns.")] public float barrelGap = 0;
    [Min(0), Tooltip("Optional amount of bullet drop (in meters per second).")]                    public float drop = 0;
    [Header("Collision:")]
    [Tooltip("Physics layers which projectile will not collide with")]                          public LayerMask ignoreLayers;
    //[Min(0), Tooltip("Wideness of projectile collision zone (zero if projectile is a point).")] public float radius = 0;
}
