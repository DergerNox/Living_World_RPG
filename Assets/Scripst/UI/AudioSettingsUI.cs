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

    private AudioManager audioManager;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        ResolveAudioManager();
    }

    private void Start()
    {
        var manager = ResolveAudioManager();
        if (manager != null)
        {
            // Set initial slider values
            masterVolumeSlider.value = manager.masterVolume;
            musicVolumeSlider.value = manager.musicVolume;
            ambientVolumeSlider.value = manager.ambientVolume;
            effectsVolumeSlider.value = manager.effectsVolume;

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
            ResolveAudioManager()?.PlayUIMenuOpen();
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);

            // Play UI menu close sound
            ResolveAudioManager()?.PlayUIMenuClose();
        }
    }

    private void OnMasterVolumeChanged(float value)
    {
        var manager = ResolveAudioManager();
        if (manager != null)
        {
            manager.masterVolume = value;
        }
    }

    private void OnMusicVolumeChanged(float value)
    {
        var manager = ResolveAudioManager();
        if (manager != null)
        {
            manager.musicVolume = value;
        }
    }

    private void OnAmbientVolumeChanged(float value)
    {
        var manager = ResolveAudioManager();
        if (manager != null)
        {
            manager.ambientVolume = value;
        }
    }

    private void OnEffectsVolumeChanged(float value)
    {
        var manager = ResolveAudioManager();
        if (manager != null)
        {
            manager.effectsVolume = value;
        }
    }

    private AudioManager ResolveAudioManager()
    {
        if (audioManager == null)
        {
            if (!ServiceLocator.TryResolve(out audioManager))
            {
                audioManager = AudioManager.Instance;
            }
        }
        return audioManager;
    }
}
