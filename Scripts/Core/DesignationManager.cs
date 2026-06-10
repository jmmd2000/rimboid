using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public enum DesignationType { Mine };

public class DesignationManager
{
    readonly List<Designation> _list = new();

    public void Add(DesignationType type, Vector2I cell)
    {
        if (Has(type, cell)) return;
        _list.Add(new Designation { Type = type, Cell = cell });
    }

    public void Remove(DesignationType type, Vector2I cell) => _list.RemoveAll(d => d.Type == type && d.Cell == cell);

    public bool Has(DesignationType type, Vector2I cell) => _list.Any(d => d.Type == type && d.Cell == cell);

    public IEnumerable<Vector2I> CellsOfType(DesignationType type) => _list.Where(d => d.Type == type).Select(d => d.Cell);
}

public class Designation
{
    public DesignationType Type;
    public Vector2I Cell;
}