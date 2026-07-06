using Godot;

/// <summary>Adds or removes cells of a grow zone.</summary>
public class Designator_GrowZone : Designator
{
    readonly GrowZone _growZone;

    public Designator_GrowZone(GrowZone growZone) => _growZone = growZone;

    public override void Apply(Vector2I cell)
    {
        if (Game.Map.Terrain[cell.X, cell.Y].Walkable && _growZone.Cells.Add(cell))
            Game.MapView.MarkGrowZone(cell);
    }

    public override void Cancel(Vector2I cell)
    {
        if (_growZone.Cells.Remove(cell))
            Game.MapView.ClearGrowZone(cell);
    }
}