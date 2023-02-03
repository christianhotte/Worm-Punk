using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Determines player health, healing & death properties.
/// </summary>
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/HealthSettings", order = 1)]
public class HealthSettings : ScriptableObject
{
    [Header("General Health Settings:")]
    [Min(1), Tooltip("Base starting health value.")] public float defaultHealth = 100;

    [Header("Regeneration:")]
    [Min(0), Tooltip("Rate (in units per second) that health regenerates (set to zero to disable regeneration).")] private float regenSpeed = 0;
    [Min(0), Tooltip("Number of seconds to wait after damage before beginning regeneration (ignore if health does not regenerate).")] private float regenPauseTime = 0;
}
