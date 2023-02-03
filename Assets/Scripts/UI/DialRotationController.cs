using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class DialRotationController : MonoBehaviour
{
    [SerializeField, Tooltip("The number of degrees to rotate the dial on one click.")] private int snapRotationAmount = 25;
    [SerializeField, Tooltip("The minimum amount that the player must rotate the dial to start registering input.")] private float angleTolerance;

    [SerializeField] private float minVal;
    [SerializeField] private float maxVal;

    [SerializeField, Tooltip("Dummy models to show when grabbing the dial.")] private GameObject leftHandModel, rightHandModel;
    [SerializeField, Tooltip("Determine if a dummy model is shown when rotating the dial.")] private bool useDummyHands;

    [SerializeField, Tooltip("The event called when the dial has been rotated, sends the angle rotation.")] private UnityEvent<float> OnValueChanged;

    private Transform dialTransform;    //The dial transform, what needs to be rotated
    private XRBaseInteractor interactor;    //The object interacting with the dial
    private float startAngle;   //The starting angle for the dial
    private bool requiresStartAngle = true; //Requires the dial to be at the start angle to rotate
    private bool shouldGetHandRotation = false; //If the dial should be checking for hand rotation

    private XRGrabInteractable grabInteractor => GetComponentInChildren<XRGrabInteractable>();  //The part of the dial that can be interacted with

    // Start is called before the first frame update
    void Start()
    {
        dialTransform = grabInteractor.transform;
    }

    private void OnEnable()
    {
        //Subscribe events for when the player grabs and releases the dial
        if(grabInteractor != null)
        {
            grabInteractor.selectEntered.AddListener(GrabStart);
            grabInteractor.selectExited.AddListener(GrabEnd);
        }
    }

    private void OnDisable()
    {
        //Removes events for when the player grabs and releases the dial
        if (grabInteractor != null)
        {
            grabInteractor.selectEntered.RemoveListener(GrabStart);
            grabInteractor.selectExited.RemoveListener(GrabEnd);
        }
    }

    /// <summary>
    /// The function that calls when something grabs the dial.
    /// </summary>
    /// <param name="args"></param>
    private void GrabStart(SelectEnterEventArgs args)
    {
        //Get the earliest interactor that is grabbing the dial
        interactor = (XRBaseInteractor)args.interactorObject;
        Debug.Log(args.interactorObject);
        Debug.Log(interactor);
        interactor.GetComponent<XRBaseControllerInteractor>().hideControllerOnSelect = true;

        //Start checking for hand rotation
        shouldGetHandRotation = true;
        startAngle = 0f;

        HandModelVisibility(true);  //Show the dummy hand model if applicable

        //Scale the dial so that it matches back to the parent after being unparented
        Vector3 newScale = ReturnToScale(dialTransform.transform.localScale);
        dialTransform.transform.SetParent(transform);
        dialTransform.transform.GetChild(0).localScale = newScale;
    }

    private Vector3 ReturnToScale(Vector3 localScale)
    {
        Vector3 newScale = localScale;

        newScale.x = 1f / localScale.x;
        newScale.y = 1f / localScale.y;
        newScale.z = 1f / localScale.x;

        return newScale;
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

        Debug.Log("Dial Grab End");

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

        //Show a different dummy hand depending on the player hand being used
        if (interactor.CompareTag("RightHand"))
            rightHandModel.SetActive(visibilityState);
        else
            leftHandModel.SetActive(visibilityState);
    }

    // Update is called once per frame
    void Update()
    {
        //If the dial should be getting rotation
        if (shouldGetHandRotation)
        {
            float rotationAngle = GetInteractorRotation();
            GetRotationDistance(rotationAngle);
        }
    }

    /// <summary>
    /// Gets current rotation of our controller.
    /// </summary>
    /// <returns></returns>
    public float GetInteractorRotation() => interactor.GetComponent<Transform>().eulerAngles.z;



    /// <summary>
    /// 
    /// </summary>
    /// <param name="currentAngle">Current controller rotation.</param>
    private void GetRotationDistance(float currentAngle)
    {
        if (!requiresStartAngle)
        {
            float angleDifference = Mathf.Abs(startAngle - currentAngle);   //The angle difference between the start angle and the current angle to see how much it's moved

            if(angleDifference > angleTolerance)
            {
                if(angleDifference > 270f) //Check to see if the user has gone from 0 to 360 degrees
                {
                    float angleCheck;

                    //Checking clockwise movement
                    if(startAngle < currentAngle)
                    {
                        angleCheck = CheckAngle(currentAngle, startAngle);

                        if (angleCheck < angleTolerance)
                            return;
                        else
                        {
                            RotateDialClockwise();
                            startAngle = currentAngle;
                        }
                    }
                    //Checking counter-clockwise movement
                    else if(startAngle > currentAngle)
                    {
                        angleCheck = CheckAngle(currentAngle, startAngle);
                        if (angleCheck < angleTolerance)
                            return;
                        else
                        {
                            RotateDialCounterClockwise();
                            startAngle = currentAngle;
                        }
                    }
                }
                //If not, just check the angle normally
                else
                {
                    //Clockwise movement
                    if(startAngle < currentAngle)
                    {
                        RotateDialCounterClockwise();
                        startAngle = currentAngle;
                    }
                    //Counter-clockwise movement
                    else if(startAngle > currentAngle)
                    {
                        RotateDialClockwise();
                        startAngle = currentAngle;
                    }
                }
            }
        }
        else
        {
            requiresStartAngle = false;
            startAngle = currentAngle;
        }
    }

    private float CheckAngle(float currentAngle, float startAngle) => (360f - currentAngle) + startAngle;

    /// <summary>
    /// Function to call event when the dial is rotated clockwise.
    /// </summary>
    private void RotateDialClockwise()
    {
        //Snap rotation of dial
        dialTransform.localEulerAngles = new Vector3(dialTransform.localEulerAngles.x, dialTransform.localEulerAngles.y + snapRotationAmount, dialTransform.localEulerAngles.z);

        Debug.Log("Rotating " + gameObject.name + " Clockwise.");
        OnValueChanged.Invoke(GetDialValue(dialTransform.localEulerAngles.y));
    }

    /// <summary>
    /// Function to call event when the dial is rotated counter-clockwise.
    /// </summary>
    private void RotateDialCounterClockwise()
    {
        //Snap rotation of dial
        dialTransform.localEulerAngles = new Vector3(dialTransform.localEulerAngles.x, dialTransform.localEulerAngles.y - snapRotationAmount, dialTransform.localEulerAngles.z);

        Debug.Log("Rotating " + gameObject.name + " Counter-Clockwise.");

        OnValueChanged.Invoke(GetDialValue(dialTransform.localEulerAngles.y));
    }

    private float GetDialValue(float angle)
    {
        return minVal + (angle / 360f * maxVal);
    }
}
