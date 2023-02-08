using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Code was used from https://youtu.be/zPZK7C5_BQo?t=781

public class LobbyUIScript : MonoBehaviour
{
    // Makes this Lobby UI manager a singleton.
    public static LobbyUIScript instance;

    [SerializeField] Menus[] menus;

    // Awake is called first thing.
    void Awake()
    {
        // Creates a static reference meaning the variable is bound to the class and not the actual object in Unity; references this script.
        instance = this;
    }

    // Easier to call the  open menu method through script
    public void OpenMenu(string menuName)
    {
        // Loops through all the menus in the Canvas.
        for (int i = 0; i < menus.Length; i++)
        {
            // If the menu matches with the menu name we're trying to open...
            if (menus[i].menuName == menuName)
            {
                // Then we can open the menu
                OpenMenu(menus[i]);
            }

            // If it's not the menu we're trying to open, then we want to close it.
            else if (menus[i].open)
            {
                CloseMenu(menus[i]);
            }
        }
    }

    // Easier to call the  open menu method through hierarchy.
    public void OpenMenu(Menus menu)
    {
        // If the menu is open, we want to close it because we only want one menu open at a time.
        for (int i = 0; i < menus.Length; i++)
        {
            CloseMenu(menus[i]);
        }

        // Opens the menu.
        menu.Open();
    }

    // Closes the menu (easier through hierarchy).
    public void CloseMenu(Menus menu)
    {
        menu.Close();
    }
}