using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class colliderTagGen : MonoBehaviour
{
    void Start()
    {
        GameObject.Find("[generated-collider-mesh]").tag = "Ground";
    }
}
