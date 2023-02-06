using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using Photon.Pun;
using Photon.Realtime;

/// <summary>
/// Manages overall player stats and abilities.
/// </summary>
public class PlayerController : MonoBehaviour, IShootable
{
    //Objects & Components:
    [Tooltip("Singleton instance of player controller.")]                                    public static PlayerController instance;
    [Tooltip("Singleton instance of this client's photonNetwork (on their NetworkPlayer).")] public static PhotonView photonView;

    [Tooltip("XROrigin component attached to player instance in scene.")] internal XROrigin xrOrigin;
    [Tooltip("Controller component for player's left hand.")]             internal ActionBasedController leftHand;
    [Tooltip("Controller component for player's right hand.")]            internal ActionBasedController rightHand;

    private Camera cam;              //Main player camera
    internal PlayerInput input;       //Input manager component used by player to send messages to hands and such
    private AudioSource audioSource; //Main player audio source

    //Settings:
    [Header("Settings Objects:")]
    [SerializeField, Tooltip("Settings determining player health properties")] private HealthSettings healthSettings;
    [Header("Debug Options:")]
    [SerializeField, Tooltip("Enables constant settings checks in order to test changes.")] private bool debugUpdateSettings;

    //Runtime Variables:
    private float currentHealth; //How much health player currently has
    private bool inCombat;  //Whether the player is actively in combat

    //RUNTIME METHODS:
    private void Awake()
    {
        //Check validity / get objects & components:
        if (instance == null) { instance = this; } else { Debug.LogError("Tried to spawn a second instance of PlayerController in scene."); Destroy(gameObject); }             //Singleton-ize player object
        if (!TryGetComponent(out input)) { Debug.LogError("PlayerController could not find PlayerInput component!"); Destroy(gameObject); }                                    //Make sure player input component is present on same object
        xrOrigin = GetComponentInChildren<XROrigin>(); if (xrOrigin == null) { Debug.LogError("PlayerController could not find XROrigin in children."); Destroy(gameObject); } //Make sure XROrigin is present inside player
        cam = GetComponentInChildren<Camera>(); if (cam == null) { Debug.LogError("PlayerController could not find camera in children."); Destroy(gameObject); }               //Make sure system has camera
        audioSource = cam.GetComponent<AudioSource>(); if (audioSource == null) audioSource = cam.gameObject.AddComponent<AudioSource>();                                      //Make sure system has an audio source

        ActionBasedController[] hands = GetComponentsInChildren<ActionBasedController>();                                    //Get both hands in player object
        if (hands[0].name.Contains("Left") || hands[0].name.Contains("left")) { leftHand = hands[0]; rightHand = hands[1]; } //First found component is on left hand
        else { rightHand = hands[0]; leftHand = hands[1]; }                                                                  //Second found component is on right hand

        //Check settings:
        if (healthSettings == null) //No health settings were provided
        {
            Debug.LogWarning("PlayerController is missing HealthSettings, using system defaults."); //Log warning in case someone forgot
            healthSettings = (HealthSettings)Resources.Load("DefaultHealthSettings");               //Load default settings from Resources folder
        }

        //Setup runtime variables:
        currentHealth = healthSettings.defaultHealth; //Set base health value

        inCombat = false;
        UpdateWeaponry();
    }
    private void Update()
    {
        if (debugUpdateSettings && Application.isEditor) //Debug settings updates are enabled (only necessary while running in Unity Editor)
        {

        }
    }

    /// <summary>
    /// Updates the weaponry so that the player can / can't fight under certain conditions.
    /// </summary>
    private void UpdateWeaponry()
    {
        //Show or hide all objects under the tag "Weapon"
        foreach (var controller in GetComponentsInChildren<ActionBasedController>())
        {
            foreach (Transform transform in controller.transform)
                if (transform.CompareTag("PlayerEquipment"))
                    transform.gameObject.SetActive(inCombat);
                if (transform.CompareTag("PlayerHand"))
                    transform.gameObject.SetActive(!inCombat);
        }
    }

    //INTERFACE METHODS:
    /// <summary>
    /// Method called when this player is hit by a projectile.
    /// </summary>
    /// <param name="projectile">The projectile which hit the player.</param>
    public void IsHit(Projectile projectile)
    {
        audioSource.PlayOneShot((AudioClip)Resources.Load("Default_Hurt_Sound")); //TEMP play hurt sound
    }

    //FUNCTIONALITY METHODS:

    public bool InCombat() => inCombat;
}
