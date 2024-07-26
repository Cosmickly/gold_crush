using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class SelectScreen : MonoBehaviour
    {
        public Slider LevelCountSlider;
        public Slider HumanCountSlider;
        public Slider AICountSlider;
        [SerializeField] private int _humanCount;
        [SerializeField] private int _aiCount;
        [SerializeField] private int _levelCount;

        private readonly int _maxPlayers = 4;
        private TextMeshProUGUI _aiCountSliderNumber;
        private TextMeshProUGUI _humanCountSliderNumber;
        private TextMeshProUGUI _levelCountSliderNumber;

        public void Awake()
        {
            LoadSettings();
        }

        private void Start()
        {
            // Ensure Number is first in Slider hierarchy
            _humanCountSliderNumber = HumanCountSlider.GetComponentInChildren<TextMeshProUGUI>();
            HumanCountSlider.onValueChanged.AddListener(delegate { HumanCountChange(); });
            HumanCountSlider.value = _humanCount;
            _humanCountSliderNumber.text = _humanCount.ToString();

            _aiCountSliderNumber = AICountSlider.GetComponentInChildren<TextMeshProUGUI>();
            AICountSlider.onValueChanged.AddListener(delegate { AICountChange(); });
            AICountSlider.value = _aiCount;
            _aiCountSliderNumber.text = _aiCount.ToString();

            _levelCountSliderNumber = LevelCountSlider.GetComponentInChildren<TextMeshProUGUI>();
            LevelCountSlider.onValueChanged.AddListener(delegate { LevelCountChange(); });
            LevelCountSlider.value = _levelCount;
            _levelCountSliderNumber.text = _levelCount.ToString();
        }

        private void LoadSettings()
        {
            _humanCount = PlayerPrefs.GetInt("HumanCount", 1);
            _aiCount = PlayerPrefs.GetInt("AIcount", 1);
            _levelCount = PlayerPrefs.GetInt("LevelCount", 3);
        }

        private void LevelCountChange()
        {
            _levelCount = (int)LevelCountSlider.value;
            _levelCountSliderNumber.text = _levelCount.ToString();
        }

        private void HumanCountChange()
        {
            _humanCount = (int)HumanCountSlider.value;
            if (_aiCount + _humanCount > _maxPlayers) AICountSlider.value = _maxPlayers - _humanCount;
            _humanCountSliderNumber.text = _humanCount.ToString();
        }

        private void AICountChange()
        {
            _aiCount = (int)AICountSlider.value;
            if (_aiCount + _humanCount > _maxPlayers) HumanCountSlider.value = _maxPlayers - _aiCount;
            _aiCountSliderNumber.text = _aiCount.ToString();
        }

        public void StartGame()
        {
            if (_levelCount < 1) _levelCount = 1;
            PlayerPrefs.SetInt("LevelCount", _levelCount);
            PlayerPrefs.SetInt("HumanCount", _humanCount);
            PlayerPrefs.SetInt("AIcount", _aiCount);

            SceneManager.LoadScene(1);
        }
    }
}