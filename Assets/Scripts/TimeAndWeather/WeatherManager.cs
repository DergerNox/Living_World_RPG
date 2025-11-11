using System;
using UnityEngine;

public enum WeatherType
{
    Clear,
    Rain,
    Storm,
    Snow,
    Fog
}

public class WeatherManager : MonoBehaviour
{
    public static WeatherManager Instance;

    public WeatherType currentWeather = WeatherType.Clear;
    public float weatherDuration = 3600f; // in-game seconds
    private float weatherTimer = 0f;

    public event Action<WeatherType> OnWeatherChanged;
    private ITimeService timeService;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        SetRandomWeather();
    }

    private void Update()
    {
        float scale = GetTimeService()?.TimeScale ?? 60f;
        weatherTimer += Time.deltaTime * scale;
        if (weatherTimer >= weatherDuration)
        {
            SetRandomWeather();
            weatherTimer = 0f;
        }
    }

    private void SetRandomWeather()
    {
        Array values = Enum.GetValues(typeof(WeatherType));
        currentWeather = (WeatherType)values.GetValue(UnityEngine.Random.Range(0, values.Length));
        OnWeatherChanged?.Invoke(currentWeather);
    }

    private ITimeService GetTimeService()
    {
        if (timeService == null)
        {
            if (!ServiceLocator.TryResolve(out timeService))
            {
                timeService = TimeManager.Instance;
            }
        }
        return timeService;
    }
}
