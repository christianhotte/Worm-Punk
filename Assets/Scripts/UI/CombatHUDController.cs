using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CombatHUDController : MonoBehaviour
{
    [SerializeField, Tooltip("The container for the information that displays the player stats information.")] private Transform playerStatsContainer;

    public void UpdatePlayerStats(PlayerStats playerStats)
    {
        playerStatsContainer.Find("PlayerDeaths").GetComponent<TextMeshProUGUI>().text = "Deaths: " + playerStats.numOfDeaths;
        playerStatsContainer.Find("PlayerKills").GetComponent<TextMeshProUGUI>().text = "Kills: " + playerStats.numOfKills;
    }
}
