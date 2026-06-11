using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>Types of player-placed work designations.</summary>
public enum DesignationType { Mine };

/// <summary>Tracks active designations on the map. Add, remove, query by type and cell.</summary>
public class DesignationManager
{
    readonly List<Designation> _list = new();

    /// <summary>Adds a designation if one doesn't already exist at that cell.</summary>
    /// <param name="type">The designation type.</param>
    /// <param name="cell">The map cell to designate.</param>
    public void Add(DesignationType type, Vector2I cell)
    {
        if (Has(type, cell)) return;
        _list.Add(new Designation { Type = type, Cell = cell });
    }

    /// <summary>Removes all designations of the given type at the cell.</summary>
    /// <param name="type">The designation type.</param>
    /// <param name="cell">The map cell to clear.</param>
    public void Remove(DesignationType type, Vector2I cell) => _list.RemoveAll(d => d.Type == type && d.Cell == cell);

    /// <summary>Checks whether a designation of the given type exists at the cell.</summary>
    /// <param name="type">The designation type.</param>
    /// <param name="cell">The map cell to check.</param>
    /// <returns>True if a matching designation exists.</returns>
    public bool Has(DesignationType type, Vector2I cell) => _list.Any(d => d.Type == type && d.Cell == cell);

    /// <summary>Returns all cells that have a designation of the given type.</summary>
    /// <param name="type">The designation type to filter by.</param>
    /// <returns>Enumerable of designated cells.</returns>
    public IEnumerable<Vector2I> CellsOfType(DesignationType type) => _list.Where(d => d.Type == type).Select(d => d.Cell);
}

/// <summary>A single designation entry: a type paired with a map cell.</summary>
public class Designation
{
    public DesignationType Type;
    public Vector2I Cell;
}