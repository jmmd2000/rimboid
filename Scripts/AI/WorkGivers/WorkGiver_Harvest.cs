using System.Collections.Generic;
using Godot;

/// <summary>Finds the nearest reachable plant to harvest, either one the player designated,
/// or a mature crop sitting in a grow zone, and returns a harvest job.</summary>
public class WorkGiver_Harvest : WorkGiver
{
    public override Job TryGiveJob(Guy guy)
    {
        var reachable = Game.Pathing.ReachableCells(guy.Cell);
        var best = NearestReachableCell(HarvestableCells(), guy, reachable, Reach.Adjacent8);
        if (best == null) return null;

        return new Job { Type = JobType.Harvest, TargetCell = best.Value, ClaimsCell = true };
    }

    /// <summary>Cells with a plant ready to harvest: player harvest designations plus mature growzone crops.</summary>
    static IEnumerable<Vector2I> HarvestableCells()
    {
        foreach (var cell in Game.Map.Designations.CellsOfType(DesignationType.Harvest))
            if (IsHarvestable(cell)) yield return cell;

        foreach (var zone in Game.Map.GrowZones.Zones)
            foreach (var cell in zone.Cells)
                if (IsHarvestable(cell)) yield return cell;
    }

    /// <summary>True if the cell holds a plant that's ready to harvest.</summary>
    static bool IsHarvestable(Vector2I cell)
    {
        var plant = Game.Map.PlantAt(cell);
        return plant != null && plant.IsHarvestable;
    }
}
