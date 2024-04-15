using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Scoreboard : MonoBehaviour
{
    public BasePlayer[] Players { get; set; }
    private TextMeshProUGUI[] _scoreUIs;
    
    private void Awake()
    {
        _scoreUIs = GetComponentsInChildren<TextMeshProUGUI>();
    }
    
    private void Start()
    {
        foreach (var player in Players)
        {
            player.onUpdateUI += UpdateUI;
        }
        
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        for (int i = 0; i < Players.Length; i++)
        {
            _scoreUIs[i].text = "P" + (i + 1) + ": " + Players[i].NumOfGold;
        }
    }

    private void OnDisable()
    {
        foreach (var player in Players)
        {
            player.onUpdateUI -= UpdateUI;
        }
    }
}
