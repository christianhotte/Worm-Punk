using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Place this on objects to make them (or parts of them) targetable by certain projectiles.
/// </summary>
public class Targetable : MonoBehaviour
{
    //Objects & Components:
    /// <summary>
    /// Master list of all targetable objects in scene.
    /// </summary>
    public static List<Targetable> targetableInstances = new List<Targetable>();

    //Settings:
    [Tooltip("The point in space which projectiles will target (leave empty to make it this object's transform.")] public Transform targetPoint;

    //RUNTIME VARIABLES:
    private void Awake()
    {
        //Initialize:
        targetableInstances.Add(this); //Add this targetable object to master list of targetable instances

        //Get objects & components:
        if (targetPoint == null) targetPoint = transform; //Set target point to self if not set in editor
    }
}
