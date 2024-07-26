using System.Collections.Generic;
using Players;
using TMPro;
using UnityEngine;

namespace UI
{
    public class Scoreboard : MonoBehaviour
    {
        public List<BasePlayer> Players = new();
        private TextMeshProUGUI[] _scoreUIs;

        private void Awake()
        {
            _scoreUIs = GetComponentsInChildren<TextMeshProUGUI>();

            foreach (var t in _scoreUIs)
                t.gameObject.SetActive(false);
        }

        private void Start()
        {
            for (var i = 0; i < Players.Count; i++) _scoreUIs[i].gameObject.SetActive(true);

            foreach (var player in Players) player.OnUpdateScore += UpdateUI;

            UpdateUI();
        }

        private void OnDisable()
        {
            foreach (var player in Players) player.OnUpdateScore -= UpdateUI;
        }

        private void UpdateUI()
        {
            for (var i = 0; i < Players.Count; i++) _scoreUIs[i].text = "P" + (i + 1) + ": " + Players[i].NumOfGold;
        }
    }
}