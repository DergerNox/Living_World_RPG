using UnityEngine;

public class LightingManager : MonoBehaviour
{
    public static LightingManager Instance;

    [Header("Lighting Settings")]
    public Light directionalLight;
    public float dayIntensity = 1f;
    public float nightIntensity = 0.3f;
    public Color dayColor = Color.white;
    public Color nightColor = new Color(0.5f, 0.5f, 1f);

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetDayLighting()
    {
        if (directionalLight != null)
        {
            directionalLight.intensity = dayIntensity;
            directionalLight.color = dayColor;
        }
    }

    public void SetNightLighting()
    {
        if (directionalLight != null)
        {
            directionalLight.intensity = nightIntensity;
            directionalLight.color = nightColor;
        }
    }

    public void UpdateLightingBasedOnTime(float timeOfDay)
    {
        // timeOfDay should be between 0 (midnight) and 1 (midnight next day)
        // Assuming day is from 0.25 to 0.75 (6am to 6pm)
        if (timeOfDay >= 0.25f && timeOfDay <= 0.75f)
        {
            SetDayLighting();
        }
        else
        {
            SetNightLighting();
        }
    }
}
