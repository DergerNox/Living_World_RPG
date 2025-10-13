using UnityEngine;

public class GraphicsOptimizer : MonoBehaviour
{
    public static GraphicsOptimizer Instance;

    [Header("Performance Settings")]
    public int targetFrameRate = 60;
    public bool enableVSync = true;
    public bool enableShadows = true;
    public ShadowResolution shadowResolution = ShadowResolution.Medium;
    public int pixelLightCount = 2;

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

        ApplySettings();
    }

    private void ApplySettings()
    {
        // Set target frame rate
        Application.targetFrameRate = targetFrameRate;

        // Enable/disable VSync
        QualitySettings.vSyncCount = enableVSync ? 1 : 0;

        // Shadow settings
        QualitySettings.shadows = enableShadows ? ShadowQuality.All : ShadowQuality.Disable;
        QualitySettings.shadowResolution = shadowResolution;

        // Pixel light count
        QualitySettings.pixelLightCount = pixelLightCount;
    }

    public void SetLowQuality()
    {
        enableShadows = false;
        shadowResolution = ShadowResolution.Low;
        pixelLightCount = 1;
        targetFrameRate = 30;
        ApplySettings();
    }

    public void SetMediumQuality()
    {
        enableShadows = true;
        shadowResolution = ShadowResolution.Medium;
        pixelLightCount = 2;
        targetFrameRate = 60;
        ApplySettings();
    }

    public void SetHighQuality()
    {
        enableShadows = true;
        shadowResolution = ShadowResolution.High;
        pixelLightCount = 4;
        targetFrameRate = 60;
        ApplySettings();
    }
}
