using System;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;

    public int hour = 0;
    public int minute = 0;
    public int day = 1;
    public int month = 1;
    public int year = 1;

    public float timeScale = 60f; // 1 real second = 1 in-game minute

    public event Action OnHourChanged;
    public event Action OnDayChanged;

    private float timer = 0f;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Update()
    {
        timer += Time.deltaTime * timeScale;
        if (timer >= 60f)
        {
            timer -= 60f;
            minute++;
            if (minute >= 60)
            {
                minute = 0;
                hour++;
                OnHourChanged?.Invoke();
                // Update lighting based on time
                if (LightingManager.Instance != null)
                {
                    float timeOfDay = (hour * 60f + minute) / 1440f; // 1440 minutes in a day
                    LightingManager.Instance.UpdateLightingBasedOnTime(timeOfDay);
                }
                if (hour >= 24)
                {
                    hour = 0;
                    day++;
                    OnDayChanged?.Invoke();
                    if (day > DaysInMonth(month, year))
                    {
                        day = 1;
                        month++;
                        if (month > 12)
                        {
                            month = 1;
                            year++;
                        }
                    }
                }
            }
        }
    }

    private int DaysInMonth(int month, int year)
    {
        switch (month)
        {
            case 2:
                return IsLeapYear(year) ? 29 : 28;
            case 4:
            case 6:
            case 9:
            case 11:
                return 30;
            default:
                return 31;
        }
    }

    private bool IsLeapYear(int year)
    {
        return (year % 4 == 0 && year % 100 != 0) || (year % 400 == 0);
    }
}
