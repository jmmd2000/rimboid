using Godot;

/// <summary>Shared day/night presentation curves.</summary>
public static class DayNight
{
    /// <summary>How night it is at a time of day (0-1): 0 through the 
    /// daylight hours, ramping to 1 across dusk and dawn.</summary>
    public static float NightFactor(float timeOfDay)
    {
        const float dawnStart = 0.22f, dawnEnd = 0.30f; // night fades out, ~5:17 to 7:12
        const float duskStart = 0.75f, duskEnd = 0.85f; // night fades in, ~18:00 to 20:24

        if (timeOfDay < 0.5f)
            return 1f - (float)Mathf.SmoothStep(dawnStart, dawnEnd, timeOfDay);
        return (float)Mathf.SmoothStep(duskStart, duskEnd, timeOfDay);
    }
}