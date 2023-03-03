using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleController : MonoBehaviour, IGrabbable
{
    [SerializeField, Tooltip("The bounds that keeps the handle within the slider.")] private Transform handleSnapPointLeft, handleSnapPointRight;

    private MeshRenderer[] handleRenderers;
    private List<Material> defaultMats = new List<Material>();
    private Material defaultMat;
    [SerializeField] private Material inRangeMat, closestOneMat, grabbedMat;

    internal event Action OnEnteredRange = delegate { };
    internal event Action OnExitRange = delegate { };
    internal event Action OnSetClosestOne = delegate { };
    internal event Action<Transform> OnStartGrabbing = delegate { };
    internal event Action OnStopGrabbing = delegate { };

    private void Awake()
    {
        handleRenderers = GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < handleRenderers.Length; i++)
            defaultMats.Add(handleRenderers[i].material);

        SetDefaultMaterials();
    }

    /// <summary>
    /// Logic for when the player is within grabbing range of the handle.
    /// </summary>
    public void EnterRange()
    {
        if(inRangeMat != null)
            SetAllMaterials(inRangeMat);
        OnEnteredRange();
    }

    /// <summary>
    /// Logic for when the player leaves the grabbing range of the handle.
    /// </summary>
    public void ExitRange()
    {
        SetDefaultMaterials();
        OnExitRange();
    }

    /// <summary>
    /// Logic for when the handle becomes the closest grabbable object.
    /// </summary>
    public void SetClosestOne()
    {
        if (closestOneMat != null)
            SetAllMaterials(closestOneMat);
        OnSetClosestOne();
    }

    /// <summary>
    /// Logic for when the player grabs the handle.
    /// </summary>
    /// <param name="handAnchor">The transform of the player's hand.</param>
    public void StartGrabbing(Transform handAnchor)
    {
        if (grabbedMat != null)
            SetAllMaterials(grabbedMat);

        OnStartGrabbing(handAnchor);
    }

    /// <summary>
    /// Logic for when the player stops grabbing the handle.
    /// </summary>
    public void StopGrabbing()
    {
        SetDefaultMaterials();
        OnStopGrabbing();
    }

    private void SetAllMaterials(Material newMat)
    {
        for(int i = 0; i < handleRenderers.Length; i++)
        {
            handleRenderers[i].material = newMat;
        }
    }

    private void SetDefaultMaterials()
    {
        for (int i = 0; i < handleRenderers.Length; i++)
        {
            handleRenderers[i].material = defaultMats[i];
        }
    }
}
