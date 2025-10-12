using UnityEngine;
using UnityEngine.UI;

public class TimeWeatherUI : MonoBehaviour
{
    public Text timeText;
    public Text weatherText;

    private void Start()
    {
        TimeManager.Instance.OnHourChanged += UpdateTimeDisplay;
        WeatherManager.Instance.OnWeatherChanged += UpdateWeatherDisplay;
        UpdateTimeDisplay();
        UpdateWeatherDisplay(WeatherManager.Instance.currentWeather);
    }

    private void UpdateTimeDisplay()
    {
        timeText.text = $"Time: {TimeManager.Instance.hour:D2}:00, Day {TimeManager.Instance.day}, Month {TimeManager.Instance.month}, Year {TimeManager.Instance.year}";
    }

    private void UpdateWeatherDisplay(WeatherType weather)
    {
        weatherText.text = $"Weather: {weather}";
    }
}
