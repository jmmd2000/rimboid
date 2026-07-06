using Godot;

/// <summary>Designates trees for chopping.</summary>
public class Designator_Chop : Designator
{
    public override void Apply(Vector2I cell)
    {
        var plant = Game.Map.PlantAt(cell);
        if (plant != null && plant.Def.WorkType == PlantWorkType.Chop && plant.IsHarvestable && !Game.Map.Designations.Has(DesignationType.Chop, cell))
        {
            Game.Map.Designations.Add(DesignationType.Chop, cell);
            Game.MapView.MarkDesignation(DesignationType.Chop, cell);
        }
    }

    public override void Cancel(Vector2I cell)
    {
        if (Game.Map.Designations.Has(DesignationType.Chop, cell))
        {
            Game.Map.Designations.Remove(DesignationType.Chop, cell);
            Game.MapView.ClearDesignation(cell);
        }
    }
}