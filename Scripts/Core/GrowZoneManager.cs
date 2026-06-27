using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>Manages all grow zones on the map.</summary>
public class GrowZoneManager
{
    readonly List<GrowZone> _zones = new();

    /// <summary>Creates a new grow zone and returns it.</summary>
    /// <returns>The new grow zone.</returns>
    public GrowZone Create()
    {
        var zone = new GrowZone();
        _zones.Add(zone);
        return zone;
    }

    /// <summary>Returns the grow zone containing the cell, or null if none.</summary>
    /// <param name="cell">The cell to look up.</param>
    /// <returns>The zone at the cell, or null. </returns>
    public GrowZone ZoneAt(Vector2I cell) => _zones.FirstOrDefault(z => z.Contains(cell));

    /// <summary>Checks whether a cell belongs to any grow zone.</summary>
    /// <param name="cell">The cell to check.</param>
    /// <returns>True if any grow zone contains this cell.</returns>
    public bool IsGrowZoneCell(Vector2I cell) => _zones.Any(z => z.Contains(cell));
}