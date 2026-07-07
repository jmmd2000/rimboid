using Godot;

/// <summary>Designates mature plants for harvesting.</summary>
public class Designator_Harvest : Designator
{
    public override void Apply(Vector2I cell)
    {
        var plant = Game.Map.PlantAt(cell);
        if (plant != null && plant.Def.WorkType == PlantWorkType.Harvest && plant.IsHarvestable && !Game.Map.Designations.Has(DesignationType.Harvest, cell))
            Game.Map.Designations.Add(DesignationType.Harvest, cell);
    }

    public override void Cancel(Vector2I cell)
    {
        if (Game.Map.Designations.Has(DesignationType.Harvest, cell))
            Game.Map.Designations.Remove(DesignationType.Harvest, cell);
    }
}