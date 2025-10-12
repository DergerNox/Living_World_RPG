using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AudioSettingsUI : MonoBehaviour
{
    public static AudioSettingsUI Instance;

    [Header("UI Elements")]
    public GameObject settingsPanel;
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider ambientVolumeSlider;
    public Slider effectsVolumeSlider;
    public Button closeButton;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (AudioManager.Instance != null)
        {
            // Set initial slider values
            masterVolumeSlider.value = AudioManager.Instance.masterVolume;
            musicVolumeSlider.value = AudioManager.Instance.musicVolume;
            ambientVolumeSlider.value = AudioManager.Instance.ambientVolume;
            effectsVolumeSlider.value = AudioManager.Instance.effectsVolume;

            // Add listeners
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            ambientVolumeSlider.onValueChanged.AddListener(OnAmbientVolumeChanged);
            effectsVolumeSlider.onValueChanged.AddListener(OnEffectsVolumeChanged);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseSettings);
        }

        // Hide panel on start
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);

            // Play UI menu open sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayUIMenuOpen();
            }
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);

            // Play UI menu close sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayUIMenuClose();
            }
        }
    }

    private void OnMasterVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.masterVolume = value;
        }
    }

    private void OnMusicVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.musicVolume = value;
        }
    }

    private void OnAmbientVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ambientVolume = value;
        }
    }

    private void OnEffectsVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.effectsVolume = value;
        }
    }
}
