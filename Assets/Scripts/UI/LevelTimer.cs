using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using Photon.Pun;

public class LevelTimer : MonoBehaviour
{
    private int levelTime;
    private float currentTime;

    private TextMeshProUGUI timerText;

    private bool timerEnded;
    public UnityEvent OnTimerEnd;

    private void Start()
    {
        SetLevelTime(60);

        timerText = GetComponent<TextMeshProUGUI>();
        currentTime = levelTime;
    }

    private void OnEnable()
    {
        OnTimerEnd.AddListener(BackToLockerRoom);
    }

    private void OnDisable()
    {
        OnTimerEnd.RemoveListener(BackToLockerRoom);
    }

    private void Update()
    {
        if(currentTime > 0)
        {
            currentTime -= Time.deltaTime;
        }
        else if(!timerEnded)
        {
            Debug.Log("Time Is Up!");
            OnTimerEnd.Invoke();
            timerEnded = true;
        }

        DisplayTime();
    }

    private void DisplayTime()
    {
        timerText.text = GetMinutes() + ":" + GetSeconds();
    }

    public void BackToLockerRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(NetworkManagerScript.instance.roomScene);
        }
    }

    public string GetMinutes() => Mathf.FloorToInt(currentTime / 60f).ToString();
    public string GetSeconds() => Mathf.FloorToInt(currentTime % 60f).ToString("00");

    public void SetLevelTime(int newLevelTime) => levelTime = newLevelTime;
}
