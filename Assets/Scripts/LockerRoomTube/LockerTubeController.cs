using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockerTubeController : MonoBehaviour
{
    [SerializeField, Tooltip("The parent that holds all of the ready lights.")] private Transform readyLights;
    internal int tubeNumber;

    private void Awake()
    {
        tubeNumber = int.Parse(name.Replace("TestTube", ""));
    }

    /// <summary>
    /// Updates the lights depending on whether the player is ready or not.
    /// </summary>
    /// <param name="isActivated">If true, the player is ready. If false, the player is not ready.</param>
    public void UpdateLights(bool isActivated)
    {
        foreach (var light in readyLights.GetComponentsInChildren<ReadyLightController>())
            light.ActivateLight(isActivated);
    }
}