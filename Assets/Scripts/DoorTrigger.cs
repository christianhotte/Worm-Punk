using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    [SerializeField, Tooltip("The animator for the door.")] private Animator DoorAnimator;

    private void OnTriggerEnter(Collider other)
    {
        //If the trigger collides with the player, raise the tube door
        if (other.CompareTag("Player"))
        {
            DoorAnimator.Play("Tube_Door_Up");
        }
    }
}
