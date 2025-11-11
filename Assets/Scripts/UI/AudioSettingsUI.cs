using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LivingWorldRPG.UI
{
    public class AudioSettingsUI : MonoBehaviour
    {
        public static AudioSettingsUI Instance { get; private set; }

        [Header("UI Elements")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider ambientVolumeSlider;
        [SerializeField] private Slider effectsVolumeSlider;
        [SerializeField] private Button closeButton;

        private AudioManager audioManager;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            ResolveAudioManager();
        }

        private void Start()
        {
            var manager = ResolveAudioManager();
            if (manager != null)
            {
                // Set initial slider values
                masterVolumeSlider.value = manager.MasterVolume;
                musicVolumeSlider.value = manager.MusicVolume;
                ambientVolumeSlider.value = manager.AmbientVolume;
                effectsVolumeSlider.value = manager.EffectsVolume;

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
                manager.MasterVolume = value;
            }
        }

        private void OnMusicVolumeChanged(float value)
        {
            var manager = ResolveAudioManager();
            if (manager != null)
            {
                manager.MusicVolume = value;
            }
        }

        private void OnAmbientVolumeChanged(float value)
        {
            var manager = ResolveAudioManager();
            if (manager != null)
            {
                manager.AmbientVolume = value;
            }
        }

        private void OnEffectsVolumeChanged(float value)
        {
            var manager = ResolveAudioManager();
            if (manager != null)
            {
                manager.EffectsVolume = value;
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
}
