using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class DialRotationController : MonoBehaviour
{
    [SerializeField] private int snapRotationAmount = 25;   //The amount of degrees to rotate the dial on one click
    [SerializeField] private float angleTolerance;  //How much to move hand in order to turn the dial

    [SerializeField] private GameObject leftHandModel, rightHandModel;
    [SerializeField] private bool useDummyHands;

    [SerializeField] private UnityEvent OnValueChanged;

    private Transform dialTransform;
    private XRBaseInteractor interactor;
    private float startAngle;
    private bool requiresStartAngle = true;
    private bool shouldGetHandRotation = false;

    private XRGrabInteractable grabInteractor;

    // Start is called before the first frame update
    void Start()
    {
        grabInteractor = GetComponent<XRGrabInteractable>();
        dialTransform = transform;
    }

    private void OnEnable()
    {
        grabInteractor.selectEntered.AddListener(GrabStart);
        grabInteractor.selectExited.AddListener(GrabEnd);
    }

    private void OnDisable()
    {
        grabInteractor.selectEntered.RemoveListener(GrabStart);
        grabInteractor.selectExited.RemoveListener(GrabEnd);
    }

    /// <summary>
    /// The function that calls when something grabs the dial.
    /// </summary>
    /// <param name="args"></param>
    private void GrabStart(SelectEnterEventArgs args)
    {
        //Get the earliest interactor that is grabbing the dial
        interactor = (XRBaseInteractor)GetComponent<XRGrabInteractable>().GetOldestInteractorSelecting();
        interactor.GetComponent<XRDirectInteractor>().hideControllerOnSelect = true;

        //Start checking for hand rotation
        shouldGetHandRotation = true;
        startAngle = 0f;

        HandModelVisibility(true);  //Show the dummy hand model if applicable
    }

    /// <summary>
    /// The function that calls when something lets go of the dial.
    /// </summary>
    /// <param name="args"></param>
    private void GrabEnd(SelectExitEventArgs args)
    {
        //Stop checking for hand rotation
        shouldGetHandRotation = false;
        requiresStartAngle = true;

        HandModelVisibility(false); //Hide the dummy hand model if applicable
    }

    /// <summary>
    /// Determine whether to show hands when turning the dial.
    /// </summary>
    /// <param name="visibilityState">Whether the dummy hands should be visible when grabbing the dial.</param>
    private void HandModelVisibility(bool visibilityState)
    {
        if (!useDummyHands)
            return;

        if (interactor.CompareTag("RightHand"))
            rightHandModel.SetActive(visibilityState);
        else
            leftHandModel.SetActive(visibilityState);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
