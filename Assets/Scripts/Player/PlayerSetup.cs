using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(PlayerController))]
public class PlayerSetup : MonoBehaviour
{
    public void SetPlayer()
    {
        SetColor(PlayerSettings.Instance.charData.testColor);
    }

    /// <summary>
    /// Set a color on the player.
    /// </summary>
    /// <param name="playerColor">The color given to the player.</param>
    public void SetColor(Color playerColor)
    {
        foreach (var controller in FindObjectsOfType<ActionBasedController>())
        {
            if (controller.GetComponentInChildren<MeshRenderer>() != null)
            {
                Debug.Log("Setting Color To " + playerColor.ToString() + " ...");
                controller.GetComponentInChildren<MeshRenderer>().material.color = playerColor;
            }
        }

        foreach(var player in FindObjectsOfType<SkinnedMeshRenderer>())
        {
            Debug.Log("Setting Player Color To " + playerColor.ToString() + " ...");
            player.material.color = playerColor;
        }
    }
}
