using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PhysicalButtonController : MonoBehaviour
{
    [SerializeField, Tooltip("Defines how far the button needs to be move to be registered as pressed. Higher number = more sensitivity.")] private float threshold = 0.1f;    //The threshold for the button that defines when the button is pressed
    [SerializeField, Tooltip("The margin of error when the button is idle so it doesn't detect incredibly small movement.")] private float deadzone = 0.025f;   // Deadzone to ensure the button doesn't rapidly press and release 

    public UnityEvent onPressed, onReleased;

    private bool isPressed;     //Checks to make sure pressed function isn't repeatedly called
    private Vector3 startPos;   //Start position of button
    private ConfigurableJoint joint;    //Joint to move button

    // Start is called before the first frame update
    void Start()
    {
        joint = GetComponentInChildren<ConfigurableJoint>();
        startPos = joint.transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        //If button is not pressed and the button is past the threshold
        if (!isPressed && GetValue() + threshold >= 1)
            Pressed();

        //If the button is pressed and the button is not past the threshold
        if (isPressed && GetValue() - threshold <= 0)
            Released();

        Vector3 buttonPos = joint.transform.localPosition;
        buttonPos.y = Mathf.Clamp(buttonPos.y, startPos.y - joint.linearLimit.limit, startPos.y);
        joint.transform.localPosition = buttonPos;
    }

    private float GetValue()
    {
        //Get the distance between the starting position and the current position of the button
        float value = Vector3.Distance(startPos, joint.transform.localPosition) / joint.linearLimit.limit;

        //If the value is less than the deadzone, reset to 0
        if (Mathf.Abs(value) < deadzone)
            value = 0;

        //Clamp to prevent weird numbers
        return Mathf.Clamp(value, -1, 1);
    }

    /// <summary>
    /// Function to call UnityEvent for pressed button.
    /// </summary>
    private void Pressed()
    {
        isPressed = true;
        onPressed.Invoke();
        Debug.Log(gameObject.name + " Pressed.");
    }

    /// <summary>
    /// Function to call UnityEvent for released button.
    /// </summary>
    private void Released()
    {
        isPressed = false;
        onReleased.Invoke();
        Debug.Log(gameObject.name + " Released.");
    }
}
