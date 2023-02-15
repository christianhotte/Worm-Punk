using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WristUIController : MonoBehaviour
{
    [SerializeField, Tooltip("The player input actions asset.")] private InputActionAsset inputActions;
    [SerializeField, Tooltip("The menu ray interactors.")] private GameObject[] rayInteractors;
    [SerializeField, Tooltip("The gameobject that shows the player HUD.")] private GameObject playerHUD;
    private Canvas wristCanvas; //The canvas that shows the wrist menu
    private InputAction menu;   //The action that activates the menu

    // Start is called before the first frame update
    void Start()
    {
        wristCanvas = GetComponent<Canvas>();   //Get the canvas component
    }

    private void OnEnable()
    {
        menu = inputActions.FindActionMap("XRI LeftHand").FindAction("Menu");   //Find the menu action from the left hand action map
        menu.Enable();
        menu.performed += ToggleMenu;
    }

    private void OnDisable()
    {
        menu.Disable();
        menu.performed -= ToggleMenu;
    }

    /// <summary>
    /// Toggles the wrist menu when pressing a button
    /// </summary>
    /// <param name="ctx">The information from the action.</param>
    public void ToggleMenu(InputAction.CallbackContext ctx)
    {
        ShowMenu(!wristCanvas.enabled); //Toggle the canvas component
    }

    /// <summary>
    /// Shows or hides the wrist menu.
    /// </summary>
    /// <param name="showMenu">If true, the menu is shown. If false, the menu is hidden.</param>
    public void ShowMenu(bool showMenu)
    {
        wristCanvas.enabled = showMenu;
        playerHUD.SetActive(showMenu);
        foreach (var interactor in rayInteractors)
            interactor.SetActive(showMenu);
    }

    /// <summary>
    /// Quits the application
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
