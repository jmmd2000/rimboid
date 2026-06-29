using System.Collections.Generic;

/// <summary>Instantaneous gauges shown in the debug overlay.</summary>
public static class Stats
{
    static readonly Dictionary<string, double> _gauges = new();

    /// <summary>Records a current value to display.</summary>
    public static void Gauge(string name, double value)
    {
        if (Prof.Enabled) _gauges[name] = value;
    }

    public static IReadOnlyDictionary<string, double> Gauges => _gauges;
}
