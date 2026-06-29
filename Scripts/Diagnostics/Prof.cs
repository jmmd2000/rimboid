using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

/// <summary>
/// Lightweight scoped profiler. Wrap a section in `using (Prof.Sample("Name"))` to profile its
/// total time, call count and worst single call for the current window
/// </summary>
public static class Prof
{
    public static bool Enabled;

    public struct Entry { public double TotalMs; public double MaxMs; public int Calls; }

    static readonly Dictionary<string, Entry> _current = new();

    /// <summary>Starts a timed scope, disposing it (end of the `using`) records the elapsed time.</summary>
    public static Scope Sample(string label) => new Scope(label);

    public readonly struct Scope : System.IDisposable
    {
        readonly string _label;
        readonly long _start;

        public Scope(string label)
        {
            _label = label;
            _start = Enabled ? Stopwatch.GetTimestamp() : 0;
        }

        public void Dispose()
        {
            if (!Enabled) return;
            double ms = (Stopwatch.GetTimestamp() - _start) * 1000.0 / Stopwatch.Frequency;
            ref var e = ref CollectionsMarshal.GetValueRefOrAddDefault(_current, _label, out _);
            e.TotalMs += ms;
            e.Calls++;
            if (ms > e.MaxMs) e.MaxMs = ms;
        }
    }

    /// <summary>Returns the accumulated samples and clears them for the next window.</summary>
    public static Dictionary<string, Entry> Snapshot()
    {
        var copy = new Dictionary<string, Entry>(_current);
        _current.Clear();
        return copy;
    }
}
