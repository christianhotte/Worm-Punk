using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(PlayerController))]
public class PlayerSetup : MonoBehaviour
{
    private CharacterData playerCharData;

    private void Awake()
    {
        playerCharData = new CharacterData();
    }
    public void SetPlayer()
    {
        SetColor(playerCharData.testColor);
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

    public CharacterData GetCharacterData() => playerCharData;
    public string CharDataToString() => JsonUtility.ToJson(playerCharData);
}

public class CharacterData
{
    public int playerID;
    public string playerName;
    public Color testColor = new Color(1, 1, 1, 1);
}
