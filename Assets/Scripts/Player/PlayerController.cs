using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages overall player stats and abilities.
/// </summary>
public class PlayerController : MonoBehaviour
{
    //Objects & Components:
    [Tooltip("Singleton instance of player controller.")]                                    public static PlayerController instance;
    [Tooltip("Singleton instance of this client's photonNetwork (on their NetworkPlayer).")] public static PhotonView photonView;

    [Tooltip("XROrigin component attached to player instance in scene.")]                    internal XROrigin xrOrigin;
    [Tooltip("Rigidbody for player's body (the part that flies around).")]                   internal Rigidbody bodyRb;
    [Tooltip("Controller component for player's left hand.")]                                internal ActionBasedController leftHand;
    [Tooltip("Controller component for player's right hand.")]                               internal ActionBasedController rightHand;
    [Tooltip("Equipment which is currently attached to the player")]                         internal List<PlayerEquipment> attachedEquipment = new List<PlayerEquipment>();

    private Camera cam;              //Main player camera
    internal PlayerInput input;      //Input manager component used by player to send messages to hands and such
    private AudioSource audioSource; //Main player audio source
    private Transform camOffset;     //Object used to offset camera position in case of weirdness
    private InputActionMap inputMap; //Input map which player uses

    //Settings:
    [Header("Settings:")]
    [SerializeField, Tooltip("Settings determining player health properties.")] private HealthSettings healthSettings;
    [Space()]
    [SerializeField, Tooltip("How far player head can get from body before it is sent back to center.")] private float maxHeadDistance;
    [Header("Sound Settings:")]
    [SerializeField, Tooltip("SFX played when player strikes a target.")] private AudioClip targetHitSound;
    [Header("Debug Options:")]
    [SerializeField, Tooltip("Enables constant settings checks in order to test changes.")]                                private bool debugUpdateSettings;
    [SerializeField, Tooltip("Enables usage of SpawnManager system to automatically position player upon instantiation.")] private bool useSpawnPoint = true;
    [SerializeField, Tooltip("Click to snap camera back to center of player rigidbody (ignoring height).")]                private bool debugCenterCamera;
    [SerializeField, Tooltip("Manually isntantiate a network player.")]                                                    private bool debugSpawnNetworkPlayer;
    [SerializeField, Tooltip("Manually destroy client network player.")]                                                   private bool debugDeSpawnNetworkPlayer;

    //Runtime Variables:
    private float currentHealth;  //How much health player currently has
    private bool inCombat;        //Whether the player is actively in combat
    private bool inMenu;          //Whether the player is actively in a menu scene
    private float timeUntilRegen; //Time (in seconds) until health regeneration can begin

    private GameObject[] weapons;   //A list of active weapons on the player
    private GameObject[] tools;     //A list of active tools on the player

    //RUNTIME METHODS:
    private void Awake()
    {
        //Check validity / get objects & components:
        if (instance == null) { instance = this; } else { Debug.LogError("Tried to spawn a second instance of PlayerController in scene."); Destroy(gameObject); }             //Singleton-ize player object
        if (!TryGetComponent(out input)) { Debug.LogError("PlayerController could not find PlayerInput component!"); Destroy(gameObject); }                                    //Make sure player input component is present on same object
        xrOrigin = GetComponentInChildren<XROrigin>(); if (xrOrigin == null) { Debug.LogError("PlayerController could not find XROrigin in children."); Destroy(gameObject); } //Make sure XROrigin is present inside player
        bodyRb = xrOrigin.GetComponent<Rigidbody>(); if (bodyRb == null) { Debug.LogError("PlayerController could not find Rigidbody on XR Origin."); Destroy(gameObject); }   //Make sure player has a rigidbody on origin
        cam = GetComponentInChildren<Camera>(); if (cam == null) { Debug.LogError("PlayerController could not find camera in children."); Destroy(gameObject); }               //Make sure system has camera
        audioSource = cam.GetComponent<AudioSource>(); if (audioSource == null) audioSource = cam.gameObject.AddComponent<AudioSource>();                                      //Make sure system has an audio source
        camOffset = cam.transform.parent;                                                                                                                                      //Get camera offset object
        inputMap = GetComponent<PlayerInput>().actions.FindActionMap("XRI Generic Interaction");                                                                               //Get generic input map from PlayerInput component

        ActionBasedController[] hands = GetComponentsInChildren<ActionBasedController>();                                    //Get both hands in player object
        if (hands[0].name.Contains("Left") || hands[0].name.Contains("left")) { leftHand = hands[0]; rightHand = hands[1]; } //First found component is on left hand
        else { rightHand = hands[0]; leftHand = hands[1]; }                                                                  //Second found component is on right hand

        //Check settings:
        if (healthSettings == null) //No health settings were provided
        {
            Debug.Log("PlayerController is missing HealthSettings, using system defaults.");          //Log warning in case someone forgot
            healthSettings = (HealthSettings)Resources.Load("DefaultSettings/DefaultHealthSettings"); //Load default settings from Resources folder
        }

        //Setup runtime variables:
        currentHealth = healthSettings.defaultHealth; //Set base health value

        weapons = GameObject.FindGameObjectsWithTag("PlayerEquipment");
        tools = GameObject.FindGameObjectsWithTag("Wall");

        inCombat = true;
        UpdateWeaponry();

        //Event subscription:
        inputMap.actionTriggered += OnInputTriggered; //Subscribe to generic input event
        SceneManager.sceneLoaded += OnSceneLoaded;    //Subscribe to scene loaded event
    }
    private void Start()
    {
        //Move to spawnpoint:
        if (SpawnManager.instance != null && useSpawnPoint) //Spawn manager is present in scene
        {
            Transform spawnpoint = SpawnManager.instance.GetSpawnPoint(); //Get spawnpoint from spawnpoint manager
            xrOrigin.transform.position = spawnpoint.position;            //Move spawned player to target position
        }
        CenterCamera(); //Make sure player camera is in dead center of rigidbody

        if (GameManager.Instance.InMenu())
        {
            inMenu = true;
            UpdateWeaponry();
        }
        else
        {
            inMenu = false;
        }
    }
    private void Update()
    {
        if (debugUpdateSettings && Application.isEditor) //Debug settings updates are enabled (only necessary while running in Unity Editor)
        {
            if (debugSpawnNetworkPlayer)
            {
                debugSpawnNetworkPlayer = false;
                NetworkPlayerSpawn.instance.SpawnNetworkPlayer();
            }
            if (debugDeSpawnNetworkPlayer)
            {
                debugDeSpawnNetworkPlayer = false;
                NetworkPlayerSpawn.instance.DeSpawnNetworkPlayer();
            }
            if (debugCenterCamera) //Camera is being manually centered
            {
                debugCenterCamera = false; //Immediately unpress button
                CenterCamera();            //Center camera to rigidbody
            }
        }

        //Update health:
        if (healthSettings.regenSpeed > 0) //Only do health regeneration if setting is on
        {
            if (timeUntilRegen > 0) { timeUntilRegen = Mathf.Max(timeUntilRegen - Time.deltaTime, 0); } //Update health regen countdown whenever relevant
            else if (currentHealth < healthSettings.defaultHealth) //Regen wait time is zero and player has lost health
            {
                currentHealth = Mathf.Min(currentHealth + (healthSettings.regenSpeed * Time.deltaTime), healthSettings.defaultHealth); //Regenerate until player is back to default health
            }
        }
    }
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CenterCamera(); //Center player camera whenever a new scene is loaded
    }
    private void OnDestroy()
    {
        //Cleanup:
        instance = null; //Clear reference on player destruction

        //Event unsubscription:
        inputMap.actionTriggered -= OnInputTriggered; //Unsubscribe from generic input event
        SceneManager.sceneLoaded -= OnSceneLoaded;    //Unsubscribe from scene loaded event
    }

    /// <summary>
    /// Uses camera offset to snap camera back to center of rigidbody.
    /// </summary>
    private void CenterCamera()
    {
        camOffset.localPosition = Vector3.zero;                                               //Zero out offset position
        Vector3 offsetAmt = xrOrigin.transform.InverseTransformPoint(cam.transform.position); //Get true amount by which camera is offset from rigidbody center
        offsetAmt.y = 0;                                                                      //Ignore discrepancy along Y axis (so operation doesn't try to flatten the player)
        camOffset.localPosition = -offsetAmt;                                                 //Use camera offset to center player to rigidbody

        print("Centering player camera."); //Indicate that camera is being centered
    }
    /// <summary>
    /// Updates the weaponry so that the player can / can't fight under certain conditions.
    /// </summary>
    private void UpdateWeaponry()
    {
        if (inMenu)
            inCombat = false;

        foreach (var weapon in weapons)
            weapon.gameObject.SetActive(inCombat);
        foreach (var tool in tools)
            tool.gameObject.SetActive(inCombat);
    }

    //INPUT METHODS:
    public void OnInputTriggered(InputAction.CallbackContext context)
    {
        switch (context.action.name) //Determine behavior based on input action
        {
            case "RightStickPress": if (context.started) { CenterCamera(); } break; //Center camera when player presses the right stick
        }
    }
    /// <summary>
    /// Called when player hits an enemy with a projectile.
    /// </summary>
    public void HitEnemy()
    {
        if (targetHitSound != null) audioSource.PlayOneShot(targetHitSound); //Play hit sound when player shoots (or damages) a target
    }
    /// <summary>
    /// Method called when this player is hit by a projectile.
    /// </summary>
    public void IsHit(int damage)
    {
        //Hit effects:
        currentHealth -= damage; //Deal projectile damage, floor at 0
        print(damage + " damage dealt");

        //Death check:
        if (currentHealth <= 0) //Player is being killed by this projectile hit
        {
            IsKilled(); //Indicate that player has been killed
        }
        else //Player is being hurt by this projectile hit
        {
            audioSource.PlayOneShot((AudioClip)Resources.Load("Sounds/Default_Hurt_Sound"));   //TEMP play hurt sound
            if (healthSettings.regenSpeed > 0) timeUntilRegen = healthSettings.regenPauseTime; //Begin regeneration sequence (of set)
        }
    }
    /// <summary>
    /// Method called when something kills this player.
    /// </summary>
    public void IsKilled()
    {
        //TEMP DEATH SEQUENCE:
        audioSource.PlayOneShot((AudioClip)Resources.Load("Sounds/Temp_Death_Sound"));
        bodyRb.velocity = Vector3.zero; //Reset player velocity
        if (SpawnManager.instance != null && useSpawnPoint) //Spawn manager is present in scene
        {
            Transform spawnpoint = SpawnManager.instance.GetSpawnPoint(); //Get spawnpoint from spawnpoint manager
            xrOrigin.transform.position = spawnpoint.position;            //Move spawned player to target position
        }
        currentHealth = healthSettings.defaultHealth; //Reset to max health
        print("Player Killed!");
    }

    //FUNCTIONALITY METHODS:

    public bool InCombat() => inCombat;
    public bool InMenu() => inMenu;
    public void SetCombat(bool combat)
    {
        inCombat = combat;
        UpdateWeaponry();
    }
    public void SetInMenu(bool menu) => inMenu = menu;
}
