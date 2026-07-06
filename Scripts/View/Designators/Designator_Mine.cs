using Godot;

/// <summary>Marks mineable rock for mining, keeping only reachable designations.</summary>
public class Designator_Mine : Designator
{
    public override void Apply(Vector2I cell)
    {
        if (Game.Map.Terrain[cell.X, cell.Y].Mineable && !Game.Map.Designations.Has(DesignationType.Mine, cell))
        {
            Game.Map.Designations.Add(DesignationType.Mine, cell);
            Game.MapView.MarkDesignation(DesignationType.Mine, cell);
        }
    }

    public override void Cancel(Vector2I cell)
    {
        if (Game.Map.Designations.Has(DesignationType.Mine, cell))
        {
            Game.Map.Designations.Remove(DesignationType.Mine, cell);
            Game.MapView.ClearDesignation(cell);
        }
    }

    public override void OnDragFinished()
    {
        foreach (var orphan in Game.Map.Designations.PruneUnreachable(Game.Map))
            Game.MapView.ClearDesignation(orphan);
    }
}