using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class MainMenu : MonoBehaviour
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

        [Header("Audio")]
        [SerializeField] private AudioMixer _audioMixer;
        [SerializeField] private Slider _musicSlider;
        [SerializeField] private TextMeshProUGUI _musicSliderLabel;
        [SerializeField] private Slider _sfxSlider;
        [SerializeField] private TextMeshProUGUI _sfxSliderLabel;

        private float _musicVolume;
        private float _sfxVolume;

        private void Start()
        {
            LoadSettings();
            _humanCountSliderLabel = HumanCountSlider.GetComponentInChildren<TextMeshProUGUI>();
            HumanCountSlider.onValueChanged.AddListener(delegate { HumanCountChange(); });
            HumanCountSlider.value = _humanCount;

            _aiCountSliderLabel = AICountSlider.GetComponentInChildren<TextMeshProUGUI>();
            AICountSlider.onValueChanged.AddListener(delegate { AICountChange(); });
            AICountSlider.value = _aiCount;

            _levelCountSliderLabel = LevelCountSlider.GetComponentInChildren<TextMeshProUGUI>();
            LevelCountSlider.onValueChanged.AddListener(delegate { LevelCountChange(); });
            LevelCountSlider.value = _levelCount;

            _musicSliderLabel = _musicSlider.GetComponentInChildren<TextMeshProUGUI>();
            _musicSlider.onValueChanged.AddListener(delegate { MusicSliderChange(); });
            _musicSlider.value = _musicVolume;

            _sfxSliderLabel = _sfxSlider.GetComponentInChildren<TextMeshProUGUI>();
            _sfxSlider.onValueChanged.AddListener(delegate { SfxSliderChange(); });
            _sfxSlider.value = _sfxVolume;
        }

        private void LoadSettings()
        {
            _humanCount = PlayerPrefs.GetInt("HumanCount", 1);
            _aiCount = PlayerPrefs.GetInt("AIcount", 1);
            _levelCount = PlayerPrefs.GetInt("LevelCount", 3);

            _musicVolume = PlayerPrefs.GetFloat("MusicVolume", 75);
            _sfxVolume = PlayerPrefs.GetFloat("SfxVolume", 75);
            _audioMixer.SetFloat("Music", SliderValueToVolume(_musicVolume));
            _audioMixer.SetFloat("SFX", SliderValueToVolume(_sfxVolume));
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

        private void MusicSliderChange()
        {
            _musicVolume = _musicSlider.value;
            float trueVolume = SliderValueToVolume(_musicVolume);
            _audioMixer.SetFloat("Music", trueVolume);
            _musicSliderLabel.text = ((int) _musicVolume).ToString();
        }

        private void SfxSliderChange()
        {
            _sfxVolume = _sfxSlider.value;
            float trueVolume = SliderValueToVolume(_sfxVolume);
            _audioMixer.SetFloat("SFX", trueVolume);
            _sfxSliderLabel.text = ((int) _sfxVolume).ToString();
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
            PlayerPrefs.SetFloat("MusicVolume", _musicSlider.value);
            PlayerPrefs.SetFloat("SfxVolume", _sfxSlider.value);
            SceneManager.LoadScene(sceneBuildIndex: 1);
        }

        private float SliderValueToVolume(float value)
        {
            return Mathf.Log10(value / 100) * 20;
        }
    }
}
