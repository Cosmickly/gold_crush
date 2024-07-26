using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace UI
{
    public class Settings : MonoBehaviour
    {
        [SerializeField] private AudioMixer _audioMixer;
        [SerializeField] private Slider _musicSlider;
        [SerializeField] private TextMeshProUGUI _musicSliderNumber;
        [SerializeField] private Slider _sfxSlider;
        [SerializeField] private TextMeshProUGUI _sfxSliderNumber;

        private float _musicVolume;
        private float _sfxVolume;

        private void Awake()
        {
            LoadSettings();
        }

        private void Start()
        {
            _musicSliderNumber = _musicSlider.GetComponentInChildren<TextMeshProUGUI>();
            _musicSlider.onValueChanged.AddListener(delegate { MusicSliderChange(); });
            _musicSlider.value = _musicVolume;

            _sfxSliderNumber = _sfxSlider.GetComponentInChildren<TextMeshProUGUI>();
            _sfxSlider.onValueChanged.AddListener(delegate { SfxSliderChange(); });
            _sfxSlider.value = _sfxVolume;
        }

        private void LoadSettings()
        {
            _musicVolume = PlayerPrefs.GetFloat("MusicVolume", 70);
            _sfxVolume = PlayerPrefs.GetFloat("SfxVolume", 70);
            _audioMixer.SetFloat("Music", SliderValueToVolume(_musicVolume));
            _audioMixer.SetFloat("SFX", SliderValueToVolume(_sfxVolume));
        }

        private void MusicSliderChange()
        {
            _musicVolume = _musicSlider.value;
            float trueVolume = SliderValueToVolume(_musicVolume);
            _audioMixer.SetFloat("Music", trueVolume);
            _musicSliderNumber.text = ((int)_musicVolume).ToString();
        }

        private void SfxSliderChange()
        {
            _sfxVolume = _sfxSlider.value;
            float trueVolume = SliderValueToVolume(_sfxVolume);
            _audioMixer.SetFloat("SFX", trueVolume);
            _sfxSliderNumber.text = ((int)_sfxVolume).ToString();
        }

        public void SaveSettings()
        {
            PlayerPrefs.SetFloat("MusicVolume", _musicSlider.value);
            PlayerPrefs.SetFloat("SfxVolume", _sfxSlider.value);
        }

        private float SliderValueToVolume(float value)
        {
            return Mathf.Log10(value / 100) * 20;
        }
    }
}