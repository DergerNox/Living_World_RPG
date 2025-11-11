using UnityEngine;
using UnityEngine.UI;

public class TimeWeatherUI : MonoBehaviour
{
    public Text timeText;
    public Text weatherText;
    private ITimeService timeService;
    private WeatherManager weatherManager;

    private void Start()
    {
        Subscribe();
        UpdateTimeDisplay();
        UpdateWeatherDisplay(GetWeatherManager()?.currentWeather ?? WeatherType.Clear);
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void UpdateTimeDisplay()
    {
        var service = GetTimeService();
        if (service == null || timeText == null) return;
        timeText.text = $"Time: {service.Hour:D2}:00, Day {service.Day}, Month {service.Month}, Year {service.Year}";
    }

    private void UpdateWeatherDisplay(WeatherType weather)
    {
        if (weatherText == null) return;
        weatherText.text = $"Weather: {weather}";
    }

    private void Subscribe()
    {
        var service = GetTimeService();
        if (service != null)
        {
            service.OnHourChanged += UpdateTimeDisplay;
        }

        var weather = GetWeatherManager();
        if (weather != null)
        {
            weather.OnWeatherChanged += UpdateWeatherDisplay;
        }
    }

    private void Unsubscribe()
    {
        if (timeService != null)
        {
            timeService.OnHourChanged -= UpdateTimeDisplay;
        }
        if (weatherManager != null)
        {
            weatherManager.OnWeatherChanged -= UpdateWeatherDisplay;
        }
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

    private WeatherManager GetWeatherManager()
    {
        if (weatherManager == null)
        {
            weatherManager = WeatherManager.Instance;
        }
        return weatherManager;
    }
}
