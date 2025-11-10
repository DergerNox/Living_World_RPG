using System;

/// <summary>
/// Encapsulates in-game time progression and notifications (ticks, days, etc.).
/// </summary>
public interface ITimeService
{
    event Action OnHourChanged;
    event Action OnDayChanged;

    int Hour { get; }
    int Minute { get; }
    int Day { get; }
    int Month { get; }
    int Year { get; }

    /// <summary>
    /// Advance time using a delta (typically Time.deltaTime).
    /// </summary>
    void Tick(float deltaTime);

    /// <summary>
    /// Directly set the absolute in-game time (useful for loading saves or debugging).
    /// </summary>
    void SetTime(int year, int month, int day, int hour, int minute);
}
