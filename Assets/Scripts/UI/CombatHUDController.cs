using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CombatHUDController : MonoBehaviour
{
    [SerializeField, Tooltip("The container for the information that displays the player stats information.")] private Transform playerStatsContainer;
    [SerializeField, Tooltip("The container for the information that displays the player stats information.")] private Transform deathInfoContainer;
    [SerializeField, Tooltip("The death information prefab.")] private DeathInfo deathInfoPrefab;

    public void UpdatePlayerStats(PlayerStats playerStats)
    {
        playerStatsContainer.Find("PlayerDeaths").GetComponent<TextMeshProUGUI>().text = "Deaths: " + playerStats.numOfDeaths;
        playerStatsContainer.Find("PlayerKills").GetComponent<TextMeshProUGUI>().text = "Kills: " + playerStats.numOfKills;
    }

    public void AddToDeathInfoBoard(string killer, string victim, Image causeOfDeath = null)
    {
        DeathInfo deathInfo = Instantiate(deathInfoPrefab, deathInfoContainer);
        deathInfo.UpdateDeathInformation(killer, victim, causeOfDeath);
        Destroy(deathInfo.gameObject, 3);
    }
}
