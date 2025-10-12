using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource backgroundMusicSource;
    public AudioSource ambientSoundSource;
    public AudioSource soundEffectsSource;

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    public float masterVolume = 1f;
    [Range(0f, 1f)]
    public float musicVolume = 0.5f;
    [Range(0f, 1f)]
    public float ambientVolume = 0.3f;
    [Range(0f, 1f)]
    public float effectsVolume = 0.7f;

    [Header("Sound Effects")]
    public AudioClip attackSound;
    public AudioClip hitSound;
    public AudioClip deathSound;
    public AudioClip victorySound;
    public AudioClip defeatSound;
    public AudioClip itemPickupSound;
    public AudioClip questCompleteSound;

    [Header("UI Sound Effects")]
    public AudioClip uiClickSound;
    public AudioClip uiMenuOpenSound;
    public AudioClip uiMenuCloseSound;

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
        if (soundEffectsSource != null && clip != null)
        {
            soundEffectsSource.PlayOneShot(clip, effectsVolume * masterVolume);
        }
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
}
