using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Determines properties of player shotgun.
/// </summary>
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/ShotgunSettings", order = 1)]
public class ShotgunSettings : ScriptableObject
{
    [Header("General:")]
    [Tooltip("Name of projectile prefab fired by this weapon (make sure this refers to a projectile in the Resources/Projectiles folder).")] public string projectileResourceName;
}
