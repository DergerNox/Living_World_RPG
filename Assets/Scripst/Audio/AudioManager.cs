using System.Diagnostics.CodeAnalysis;
using UnityEngine;

[SuppressMessage("Design", "CA1050:DeclareTypesInNamespaces", Justification = "Unity MonoBehaviour root-level type.")]
public class AudioManager : MonoBehaviour, IAudioService
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource backgroundMusicSource;
    [SerializeField] private AudioSource ambientSoundSource;
    [SerializeField] private AudioSource soundEffectsSource;

    [Header("Volume Settings")]
    [SerializeField][Range(0f, 1f)] private float masterVolume = 1f;
    [SerializeField][Range(0f, 1f)] private float musicVolume = 0.5f;
    [SerializeField][Range(0f, 1f)] private float ambientVolume = 0.3f;
    [SerializeField][Range(0f, 1f)] private float effectsVolume = 0.7f;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip victorySound;
    [SerializeField] private AudioClip defeatSound;
    [SerializeField] private AudioClip itemPickupSound;
    [SerializeField] private AudioClip questCompleteSound;

    [Header("UI Sound Effects")]
    [SerializeField] private AudioClip uiClickSound;
    [SerializeField] private AudioClip uiMenuOpenSound;
    [SerializeField] private AudioClip uiMenuCloseSound;

    [Header("Pooling")]
    [SerializeField][Range(1, 32)] private int sfxPoolSize = 8;
    private AudioSource[] _sfxPool;
    private int _sfxPoolIndex;

    public float MasterVolume
    {
        get => masterVolume;
        set => masterVolume = Mathf.Clamp01(value);
    }

    public float MusicVolume
    {
        get => musicVolume;
        set => musicVolume = Mathf.Clamp01(value);
    }

    public float AmbientVolume
    {
        get => ambientVolume;
        set => ambientVolume = Mathf.Clamp01(value);
    }

    public float EffectsVolume
    {
        get => effectsVolume;
        set => effectsVolume = Mathf.Clamp01(value);
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // Ensure AudioSources are set up
        if (backgroundMusicSource == null)
        {
            backgroundMusicSource = gameObject.AddComponent<AudioSource>();
            backgroundMusicSource.loop = true;
        }

        if (ambientSoundSource == null)
        {
            ambientSoundSource = gameObject.AddComponent<AudioSource>();
            ambientSoundSource.loop = true;
        }

        if (soundEffectsSource == null)
        {
            soundEffectsSource = gameObject.AddComponent<AudioSource>();
        }

        // Create SFX pool for overlapping one-shots
        _sfxPool = new AudioSource[sfxPoolSize];
        for (int i = 0; i < sfxPoolSize; i++)
        {
            var src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            _sfxPool[i] = src;
        }

        UpdateVolumes();
    }

    private void Update()
    {
        UpdateVolumes();
    }

    private void UpdateVolumes()
    {
        if (backgroundMusicSource != null)
        {
            backgroundMusicSource.volume = masterVolume * musicVolume;
        }

        if (ambientSoundSource != null)
        {
            ambientSoundSource.volume = masterVolume * ambientVolume;
        }

        if (soundEffectsSource != null)
        {
            soundEffectsSource.volume = masterVolume * effectsVolume;
        }
        if (_sfxPool != null)
        {
            for (int i = 0; i < _sfxPool.Length; i++)
            {
                if (_sfxPool[i] != null) _sfxPool[i].volume = masterVolume * effectsVolume;
            }
        }
    }

    public void PlayBackgroundMusic(AudioClip clip)
    {
        if (backgroundMusicSource != null && clip != null)
        {
            backgroundMusicSource.clip = clip;
            backgroundMusicSource.Play();
        }
    }

    public void StopBackgroundMusic()
    {
        if (backgroundMusicSource != null)
        {
            backgroundMusicSource.Stop();
        }
    }

    public void PlayAmbientSound(AudioClip clip, bool loop = true)
    {
        if (ambientSoundSource != null && clip != null)
        {
            ambientSoundSource.clip = clip;
            ambientSoundSource.loop = loop;
            ambientSoundSource.Play();
        }
    }

    public void StopAmbientSound()
    {
        if (ambientSoundSource != null)
        {
            ambientSoundSource.Stop();
        }
    }

    public void PlayOneShotAmbient(AudioClip clip)
    {
        if (ambientSoundSource != null && clip != null)
        {
            ambientSoundSource.PlayOneShot(clip, ambientVolume * masterVolume);
        }
    }

    public void PlaySoundEffect(AudioClip clip)
    {
        if (clip == null) return;
        var src = NextSfxSource();
        if (src != null) src.PlayOneShot(clip, effectsVolume * masterVolume);
    }

    public void PlayAttackSound()
    {
        PlaySoundEffect(attackSound);
    }

    public void PlayHitSound()
    {
        PlaySoundEffect(hitSound);
    }

    public void PlayDeathSound()
    {
        PlaySoundEffect(deathSound);
    }

    public void PlayVictorySound()
    {
        PlaySoundEffect(victorySound);
    }

    public void PlayDefeatSound()
    {
        PlaySoundEffect(defeatSound);
    }

    public void PlayItemPickupSound()
    {
        PlaySoundEffect(itemPickupSound);
    }

    public void PlayQuestCompleteSound()
    {
        PlaySoundEffect(questCompleteSound);
    }

    public void PlayUIClick()
    {
        PlaySoundEffect(uiClickSound);
    }

    public void PlayUIMenuOpen()
    {
        PlaySoundEffect(uiMenuOpenSound);
    }

    public void PlayUIMenuClose()
    {
        PlaySoundEffect(uiMenuCloseSound);
    }

    public void PlayAmbientTrack(string trackId)
    {
        if (string.IsNullOrEmpty(trackId))
        {
            Debug.LogWarning("PlayAmbientTrack called with empty trackId.");
            return;
        }

        var clip = UnityEngine.Resources.Load<AudioClip>(trackId);
        if (clip == null)
        {
            Debug.LogWarning($"Ambient track not found at Resources path: {trackId}");
            return;
        }

        PlayAmbientSound(clip);
    }

    private AudioSource NextSfxSource()
    {
        if (_sfxPool == null || _sfxPool.Length == 0) return soundEffectsSource;
        _sfxPoolIndex++;
        if (_sfxPoolIndex >= _sfxPool.Length) _sfxPoolIndex = 0;
        return _sfxPool[_sfxPoolIndex];
    }
}
