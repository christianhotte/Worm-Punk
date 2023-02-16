using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUDController : MonoBehaviour
{
    public enum HUD_MENU_STATE { Main, Audio, Gameplay, Controls }

    [SerializeField, Tooltip("Main settings menu game object.")] private GameObject mainMenu;
    [SerializeField, Tooltip("Audio settings menu game object.")] private GameObject audioMenu;
    [SerializeField, Tooltip("Gameplay settings menu game object.")] private GameObject gameplayMenu;
    [SerializeField, Tooltip("Controls settings menu game object.")] private GameObject controlsMenu;

    [Header("Menu First Selected Items")]
    [SerializeField, Tooltip("All of the buttons on the main settings menu page.")] private Selectable[] mainMenuButtons;
    [SerializeField, Tooltip("The selectable for the audio menu page.")] private Selectable audioMenuSelected;
    [SerializeField, Tooltip("The selectable for the gameplay menu page.")] private Selectable gameplayMenuSelected;
    [SerializeField, Tooltip("The selectable for the controls menu page.")] private Selectable controlsMenuSelected;

    private GameObject currentMenu; //The GameObject for the current active menu

    private void OnEnable()
    {
        currentMenu = mainMenu;
    }

    private void SwitchMenu(HUD_MENU_STATE newMenuState)
    {
        GameObject newMenu = currentMenu;

        switch (newMenuState)
        {
            case HUD_MENU_STATE.Audio:
                currentMenu = audioMenu;
                break;
            case HUD_MENU_STATE.Gameplay:
                currentMenu = gameplayMenu;
                break;
            case HUD_MENU_STATE.Controls:
                currentMenu = controlsMenu;
                break;
            default:
                currentMenu = mainMenu;
                break;
        }

        GameObject prevMenu = currentMenu;

        currentMenu = newMenu;
        currentMenu.SetActive(true);
    }
}
