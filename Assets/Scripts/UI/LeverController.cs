using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LeverController : MonoBehaviour
{
    public enum HingeJointState { Min, Max, None }

    [SerializeField, Tooltip("Angle Threshold If Limit Is Reached")] float angleBetweenThreshold = 1f;
    public HingeJointState hingeJointState = HingeJointState.None;  //The state of the hinge joint
    private HingeJoint hinge;

    [SerializeField, Tooltip("If true, the lever locks when a limit is reached.")] private bool lockOnMinimumLimit, lockOnMaximumLimit;
    private bool isLocked = false;  //Checks to see if the lever is locked in place

    [SerializeField, Tooltip("The minimum numerical value of the lever.")] private float minimumValue = -1f;
    [SerializeField, Tooltip("The maximum numerical value of the lever.")] private float maximumValue = 1f;

    [Tooltip("The event called when the minimum limit of the lever is reached.")] public UnityEvent OnMinLimitReached;
    [Tooltip("The event called when the maximum limit of the lever is reached.")] public UnityEvent OnMaxLimitReached;
    [Tooltip("The event called when the lever is moved.")] public UnityEvent<float> OnValueChanged;

    private float previousValue, currentValue;  //The previous and current frame's value of the lever

    // Start is called before the first frame update
    void Start()
    {
        hinge = GetComponentInChildren<HingeJoint>();
    }

    private void FixedUpdate()
    {
        //If the lever is not locked, check its angle
        if (!isLocked)
        {
            float angleWithMinLimit = Mathf.Abs(hinge.angle - hinge.limits.min);
            float angleWithMaxLimit = Mathf.Abs(hinge.angle - hinge.limits.max);

            //If the angle has hit the minimum limit and is not already at the limit
            if (angleWithMinLimit < angleBetweenThreshold)
            {
                if (hingeJointState != HingeJointState.Max)
                {
                    Debug.Log(transform.name + "Maximum Limit Reached.");
                    OnMaxLimitReached.Invoke();

                    //Move the hinge to the upper limit
                    hinge.transform.localEulerAngles = new Vector3(hinge.limits.min, hinge.transform.localEulerAngles.y, hinge.transform.localEulerAngles.z);

                    if (lockOnMaximumLimit)
                    {
                        LockLever(true);
                    }
                }

                hingeJointState = HingeJointState.Max;
            }
            //If the angle has hit the maximum limit and is not already at the limit
            else if (angleWithMaxLimit < angleBetweenThreshold)
            {
                if (hingeJointState != HingeJointState.Min)
                {
                    Debug.Log(transform.name + "Minimum Limit Reached.");
                    OnMaxLimitReached.Invoke();

                    //Move the hinge to the lower limit
                    hinge.transform.localEulerAngles = new Vector3(hinge.limits.max, hinge.transform.localEulerAngles.y, hinge.transform.localEulerAngles.z);

                    if (lockOnMinimumLimit)
                    {
                        LockLever(true);
                    }
                }

                hingeJointState = HingeJointState.Min;
            }
            else
            {
                hingeJointState = HingeJointState.None;
            }
        }

        currentValue = GetLeverValue(); //Get the value of the lever

        //If the value has changed since the previous frame, call the OnValueChanged event
        if(currentValue != previousValue)
        {
            OnValueChanged.Invoke(currentValue);
            previousValue = currentValue;
        }
    }

    /// <summary>
    /// Gets the lever value based on the given minimum and maximum values.
    /// </summary>
    /// <returns>The current numerical value of the lever based on its position.</returns>
    public float GetLeverValue()
    {
        float maxValueDistance = Mathf.Abs(minimumValue - maximumValue);
        float currentDistance = (hinge.limits.max - hinge.angle) / (hinge.limits.max - hinge.limits.min);
        return minimumValue + (maxValueDistance * currentDistance);
    }

    /// <summary>
    /// Locks or unlocks the lever's movement.
    /// </summary>
    /// <param name="lockLever">If true, the lever is locked. If false, the lever is unlocked.</param>
    public void LockLever(bool lockLever)
    {
        isLocked = lockLever;
        hinge.gameObject.GetComponent<Rigidbody>().freezeRotation = lockLever;
    }
}
