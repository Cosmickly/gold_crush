using System;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class SelectScreen : MonoBehaviour
    {
        public Slider LevelCountSlider;
        private TextMeshProUGUI _levelCountSliderLabel;
        public Slider HumanCountSlider;
        private TextMeshProUGUI _humanCountSliderLabel;
        public Slider AICountSlider;
        private TextMeshProUGUI _aiCountSliderLabel;

        private readonly int _maxPlayers = 4;
        [SerializeField] private int _humanCount;
        [SerializeField] private int _aiCount;
        [SerializeField] private int _levelCount;

        public void Awake()
        {
            LoadSettings();
        }

        private void Start()
        {
            _humanCountSliderLabel = HumanCountSlider.GetComponentInChildren<TextMeshProUGUI>();
            HumanCountSlider.onValueChanged.AddListener(delegate { HumanCountChange(); });
            HumanCountSlider.value = _humanCount;

            _aiCountSliderLabel = AICountSlider.GetComponentInChildren<TextMeshProUGUI>();
            AICountSlider.onValueChanged.AddListener(delegate { AICountChange(); });
            AICountSlider.value = _aiCount;

            _levelCountSliderLabel = LevelCountSlider.GetComponentInChildren<TextMeshProUGUI>();
            LevelCountSlider.onValueChanged.AddListener(delegate { LevelCountChange(); });
            LevelCountSlider.value = _levelCount;
        }

        private void LoadSettings()
        {
            _humanCount = PlayerPrefs.GetInt("HumanCount", 1);
            _aiCount = PlayerPrefs.GetInt("AIcount", 1);
            _levelCount = PlayerPrefs.GetInt("LevelCount", 3);

        }

        private void LevelCountChange()
        {
            _levelCount = (int) LevelCountSlider.value;
            _levelCountSliderLabel.text = _levelCount.ToString();
        }
        
        private void HumanCountChange()
        {
            _humanCount = (int) HumanCountSlider.value;
            if (_aiCount + _humanCount > _maxPlayers) AICountSlider.value = _maxPlayers - _humanCount;
            _humanCountSliderLabel.text = _humanCount.ToString();
        }
        
        private void AICountChange()
        {
            _aiCount = (int) AICountSlider.value;
            if (_aiCount + _humanCount > _maxPlayers) HumanCountSlider.value = _maxPlayers - _aiCount;
            _aiCountSliderLabel.text = _aiCount.ToString();
        }

        public void StartGame()
        {
            if (_levelCount < 1) _levelCount = 1;
            PlayerPrefs.SetInt("LevelCount", _levelCount);
            PlayerPrefs.SetInt("HumanCount", _humanCount);
            PlayerPrefs.SetInt("AIcount", _aiCount);

            SceneManager.LoadScene(sceneBuildIndex: 1);
        }
    }
}
