using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>A finished, placed building. Blocks movement when its def says so.</summary>
public class Building
{
    public BuildingDef Def { get; init; }
    public Vector2I Cell { get; init; } // origin (top left cell) of the footprint
    public int Rotation { get; init; }

    /// <summary>Every cell this building covers, derived from its origin and the def's size.</summary>
    public IEnumerable<Vector2I> OccupiedCells => Footprint.Cells(Cell, Def.Size, Rotation);

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

    /// <summary>The first component of the given type, or null </summary>
    public T GetComponent<T>() where T : BuildingComponent => Components.OfType<T>().FirstOrDefault();

    /// <summary>The workbench component if this building has one, else null </summary>
    public BuildingComponent_WorkBench WorkBench => GetComponent<BuildingComponent_WorkBench>();
}