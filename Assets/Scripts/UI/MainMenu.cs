using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class MainMenu : MonoBehaviour
    {
        public Slider LevelCountSlider;
        public TextMeshProUGUI LevelCountSliderLabel;
        public Slider HumanCountSlider;
        public TextMeshProUGUI HumanCountSliderLabel;
        public Slider AICountSlider;
        public TextMeshProUGUI AICountSliderLabel;

        private readonly int _maxPlayers = 4;
        [SerializeField] private int _humanCount;
        [SerializeField] private int _aiCount;
        [SerializeField] private int _levelCount;

        private void Start()
        {
            HumanCountSliderLabel = HumanCountSlider.GetComponentInChildren<TextMeshProUGUI>();
            HumanCountSlider.onValueChanged.AddListener(delegate { HumanCountChange(); });
            _humanCount = PlayerPrefs.GetInt("HumanCount", 1);
            HumanCountSlider.value = _humanCount;
            
            AICountSliderLabel = AICountSlider.GetComponentInChildren<TextMeshProUGUI>();
            AICountSlider.onValueChanged.AddListener(delegate { AICountChange(); });
            _aiCount = PlayerPrefs.GetInt("AIcount", 0);
            AICountSlider.value = _aiCount;
            
            LevelCountSliderLabel = LevelCountSlider.GetComponentInChildren<TextMeshProUGUI>();
            LevelCountSlider.onValueChanged.AddListener(delegate { LevelCountChange(); });
            _levelCount = PlayerPrefs.GetInt("LevelCount", 1);
            LevelCountSlider.value = _levelCount;
        }

        private void LevelCountChange()
        {
            _levelCount = (int) LevelCountSlider.value;
            LevelCountSliderLabel.text = _levelCount.ToString();
        }
        
        private void HumanCountChange()
        {
            _humanCount = (int) HumanCountSlider.value;
            if (_aiCount + _humanCount > _maxPlayers) AICountSlider.value = _maxPlayers - _humanCount;
            HumanCountSliderLabel.text = _humanCount.ToString();
        }
        
        private void AICountChange()
        {
            _aiCount = (int) AICountSlider.value;
            if (_aiCount + _humanCount > _maxPlayers) HumanCountSlider.value = _maxPlayers - _aiCount;
            AICountSliderLabel.text = _aiCount.ToString();
        }

        public void Quit()
        {
            Application.Quit();
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
