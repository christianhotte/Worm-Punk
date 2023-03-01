using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class DeathInfo : MonoBehaviour
{
    private TextMeshProUGUI killerName;
    private Image icon;
    private TextMeshProUGUI victimName;

    // Start is called before the first frame update
    void Start()
    {
        killerName = transform.Find("KillerName").GetComponent<TextMeshProUGUI>();
        icon = transform.Find("Icon").GetComponent<Image>();
        victimName = transform.Find("VictimName").GetComponent<TextMeshProUGUI>();
    }

    /// <summary>
    /// Updates the death information on the board.
    /// </summary>
    /// <param name="killer"></param>
    /// <param name="victim"></param>
    /// <param name="causeOfDeath"></param>
    public void UpdateDeathInformation(string killer, string victim, Image causeOfDeath)
    {
        killerName.text = killer;
        victimName.text = victim;

        if (causeOfDeath != null)
            icon = causeOfDeath;
        else
            icon.color = new Color(0, 0, 0, 0);
    }
}
