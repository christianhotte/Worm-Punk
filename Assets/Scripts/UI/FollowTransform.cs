using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    [SerializeField] private Transform lookAt;
    [SerializeField] private Transform transformToFollow;
    [SerializeField] private float followSpeed;

    private Transform objectTransform;

    // Start is called before the first frame update
    void Start()
    {
        objectTransform = transform;
    }

    // Update is called once per frame
    void Update()
    {
        objectTransform.LookAt(lookAt, Vector3.up);

        Vector3 newPos = objectTransform.position;
        Vector3 followPos = transformToFollow.position;
        newPos.x = Mathf.Lerp(newPos.x, followPos.x, followSpeed * Time.deltaTime);
        newPos.y = Mathf.Lerp(newPos.y, followPos.y, followSpeed * Time.deltaTime);
        newPos.z = Mathf.Lerp(newPos.z, followPos.z, followSpeed * Time.deltaTime);
        transform.position = newPos;
    }
}
