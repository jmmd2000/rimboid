using System.Collections.Generic;
using Godot;

/// <summary>A finished, placed building. Blocks movement when its def says so.</summary>
public class Building
{
    public BuildingDef Def { get; init; }
    public Vector2I Cell { get; init; } // origin (top left cell) of the footprint
    public int Rotation { get; init; }

    /// <summary>Every cell this building covers, derived from its origin and the def's size.</summary>
    public IEnumerable<Vector2I> OccupiedCells => Footprint.Cells(Cell, Def.Size, Rotation);

    /// <summary>The centroid of the occupied cells, in cell space (a 1x2 at (5,5) gives (5.5, 6.0)). Views
    /// scale this by tile size to place things over the footprint, e.g. the deconstruct marker or a light.</summary>
    public Vector2 FootprintCentre
    {
        get
        {
            var sum = Vector2.Zero;
            int count = 0;
            foreach (var c in OccupiedCells) { sum += new Vector2(c.X + 0.5f, c.Y + 0.5f); count++; }
            return sum / count;
        }
    }

    /// <summary>Runtime components built from the def's ComponentProperties</summary>
    public List<BuildingComponent> Components { get; } = new();

    /// <summary> Builds the runtime components from the def. Call once after Def/Cell/Rotation are set.</summary>
    public void InitComponents()
    {
        if (Def.Components == null) return;
        foreach (var props in Def.Components)
        {
            var component = props.MakeComponent();
            component.Building = this;
            component.Props = props;
            Components.Add(component);
        }
    }

    /// <summary>The first component of the given type, or null. Manual loop, no LINQ — this runs per-frame
    /// from the bill UI, so it avoids the enumerator allocation.</summary>
    public T GetComponent<T>() where T : BuildingComponent
    {
        foreach (var component in Components)
            if (component is T match) return match;
        return null;
    }

    /// <summary>The workbench component if this building has one, else null </summary>
    public BuildingComponent_WorkBench WorkBench => GetComponent<BuildingComponent_WorkBench>();
}