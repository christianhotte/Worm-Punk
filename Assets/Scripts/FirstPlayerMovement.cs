using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Code was used from https://www.youtube.com/watch?v=_QajrabyTJc

public class FirstPlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public float speed = 12f;

    // Update is called once per frame
    void Update()
    {
        // Player movements from WASD input.
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        // Moves the player
        controller.Move(move * speed * Time.deltaTime);
    }
}