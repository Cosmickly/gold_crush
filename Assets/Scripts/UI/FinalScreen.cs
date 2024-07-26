using System;
using Players;
using TMPro;
using UnityEngine;

namespace UI
{
    public class FinalScreen : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _finalText;

        public void SetText(BasePlayer[] players)
        {
            Array.Sort(players, (x, y) => y.NumOfGold.CompareTo(x.NumOfGold));
            foreach (var player in players)
                _finalText.text += "Player " + (player.PlayerId + 1) + ": " + player.NumOfGold + "\n";
        }
    }
}