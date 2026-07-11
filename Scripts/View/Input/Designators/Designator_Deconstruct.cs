using Godot;

/// <summary>Marks a placed building for deconstruction, designating its origin cell so a
/// multi-cell building gets a single designation</summary>
public class Designator_Deconstruct : Designator
{
    public override void Apply(Vector2I cell)
    {
        var building = Game.Map.BuildingAt(cell);
        if (building != null && !Game.Map.Designations.Has(DesignationType.Deconstruct, building.Cell))
            Game.Map.Designations.Add(DesignationType.Deconstruct, building.Cell);
    }

    public override void Cancel(Vector2I cell)
    {
        var building = Game.Map.BuildingAt(cell);
        if (building != null)
            Game.Map.Designations.Remove(DesignationType.Deconstruct, building.Cell);
    }
}