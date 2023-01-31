using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using Photon.Pun;

// This script was used from https://youtu.be/KHWuTBmT1oI?t=1511

public class NetworkPlayer : MonoBehaviour
{
    // Declaring the player's VR movements
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;

    private GameObject XROrigin;
    private PhotonView photonView;

    private Transform headRig;
    private Transform leftHandRig;
    private Transform rightHandRig;

    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();

        // Gets the network player to move with the player instead of just moving locally.
        XROrigin = GameObject.Find("XR Origin");
        headRig = XROrigin.transform.Find("Camera Offset/Main Camera");
        leftHandRig = XROrigin.transform.Find("Camera Offset/LeftHand Controller");
        rightHandRig = XROrigin.transform.Find("Camera Offset/RightHand Controller");
    }

    // Update is called once per frame
    void Update()
    {
        // Synchronizes the player over the network.
        if (photonView.IsMine)
        {
            /* Disables the network player's camera & hands so that it uses the XR Origin's.
            rightHand.gameObject.SetActive(false);
            leftHand.gameObject.SetActive(false);
            head.gameObject.SetActive(false);*/

            // Calls these functions to map the position of the player's hands & headset
            MapPosition(head, headRig);
            MapPosition(leftHand, leftHandRig);
            MapPosition(rightHand, rightHandRig);

            // Disables all of the renderers in the Network player so that we can just render the XR Origin's.
            if (photonView.IsMine)
            {
                foreach (var item in GetComponentsInChildren<Renderer>())
                {
                    item.enabled = false;
                }
            }
        }
    }

    // This synchronizes the positions of the headset & the hand controllers 
    void MapPosition(Transform target, Transform rigTransform)
    {
        /*InputDevices.GetDeviceAtXRNode(node).TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 position);
        InputDevices.GetDeviceAtXRNode(node).TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotation);*/

        target.position = rigTransform.position;
        target.rotation = rigTransform.rotation;
    }
}