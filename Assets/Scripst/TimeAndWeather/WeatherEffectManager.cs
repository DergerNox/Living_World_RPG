using UnityEngine;

public class WeatherEffectManager : MonoBehaviour
{
    public static WeatherEffectManager Instance;

    public ParticleSystem rainParticles;
    public ParticleSystem snowParticles;
    public GameObject fogOverlay;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        WeatherManager.Instance.OnWeatherChanged += UpdateWeatherEffects;
        UpdateWeatherEffects(WeatherManager.Instance.currentWeather);
    }

    private void UpdateWeatherEffects(WeatherType weather)
    {
        // Disable all effects first
        if (rainParticles != null) rainParticles.Stop();
        if (snowParticles != null) snowParticles.Stop();
        if (fogOverlay != null) fogOverlay.SetActive(false);

        // Enable based on weather
        switch (weather)
        {
            case WeatherType.Rain:
                if (rainParticles != null) rainParticles.Play();
                break;
            case WeatherType.Snow:
                if (snowParticles != null) snowParticles.Play();
                break;
            case WeatherType.Fog:
                if (fogOverlay != null) fogOverlay.SetActive(true);
                break;
            case WeatherType.Storm:
                if (rainParticles != null) rainParticles.Play();
                // Add lightning or something, but for simplicity, just rain
                break;
        }
    }
}
