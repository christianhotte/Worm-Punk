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
    [Min(0), Tooltip("How much force this projectile applies to struck objects.")] public float knockback = 0;
    [Header("Travel Properties:")]
    [Tooltip("Speed at which projectile travels upon spawn.")]                                     public float initialVelocity;
    [Tooltip("Maximum distance projectile can travel (leave zero to make infinite).")]             public float range = 0;
    [Min(0), Tooltip("Distance in front of barrel position at which projectile actually spawns.")] public float barrelGap = 0;
    [Min(0), Tooltip("Optional amount of bullet drop (in meters per second).")]                    public float drop = 0;
    [Header("Targeting:")]
    [Min(0), Tooltip("How intensely projectiles home in toward targets (set to zero to disable homing).")] public float homingStrength;
    [MinMaxSlider(0, 180), Tooltip("Max angle between projectile and target at which projectile will lock on (second part is angle at which projectile will disregard targets entirely).")] public Vector2 targetDesignationAngle;
    [Min(0), Tooltip("Maximum distance at which projectile can acquire a target.")]                                                                                  public float targetingDistance;
    [Tooltip("Require line-of-sight for active targeting.")]                                                                                                         public bool LOSTargeting;
    [Tooltip("Sets projectile target acquisition to always run, even when a target has already been found.")]                                                        public bool alwaysLookForTarget;
    [Range(0, 3), Tooltip("Projectile will use target velocity to predict movement and attempt interception, increase iterations for better accuracy (expensive).")] public int predictionIterations;
    [Range(0, 1), Tooltip("Slide to the left to prioritize easier-to-hit targets, slide to the right to prioritize closer targets.")]                                public float angleDistancePreference;
    [Tooltip("Layers to ignore when raycasting for line-of-sight targeting.")]                                                                                       public LayerMask targetingIgnoreLayers;
    [Min(1), Tooltip("Number of times per second projectile runs targeting function during target acquisition period.")]                                             public int targetingTickRate = 30;
    [Header("Collision:")]
    [Tooltip("Physics layers which projectile will not collide with.")] public LayerMask ignoreLayers;
    //[Min(0), Tooltip("Wideness of projectile collision zone (zero if projectile is a point).")] public float radius = 0;
}
