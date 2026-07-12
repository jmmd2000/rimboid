using Godot;

/// <summary>Marks a placed building for deconstruction (its origin cell, so a multi-cell building
/// gets one designation), or deletes it outright in creative mode.</summary>
public class Designator_Deconstruct : Designator
{
    public override void Apply(Vector2I cell)
    {
        var building = Game.Map.BuildingAt(cell);
        if (building == null) return;

        // creative mode: delete it outright, no colonist, no materials returned
        if (Game.CreativeMode)
        {
            Game.Map.RemoveBuilding(building);
            foreach (var c in building.OccupiedCells) Game.Pathing.RefreshCell(Game.Map, c);
            return;
        }

        if (!Game.Map.Designations.Has(DesignationType.Deconstruct, building.Cell))
            Game.Map.Designations.Add(DesignationType.Deconstruct, building.Cell);
    }

    public override void Cancel(Vector2I cell)
    {
        var building = Game.Map.BuildingAt(cell);
        if (building != null)
            Game.Map.Designations.Remove(DesignationType.Deconstruct, building.Cell);
    }
}