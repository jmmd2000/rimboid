using System.Collections.Generic;
using Godot;

/// <summary>Finds the nearest reachable empty cell in a grow zone and returns a sow job.</summary>
public class WorkGiver_Sow : WorkGiver
{

    public override WorkType Work => WorkType.Sow;

    public override Job TryGiveJob(Guy guy)
    {
        var reachable = Game.Pathing.ReachableCells(guy.Cell);
        var best = NearestReachableCell(SowableCells(), guy, reachable, Reach.OnCell);
        if (best == null) return null;

        return new Job { Type = JobType.Sow, TargetCell = best.Value, ClaimsCell = true };
    }

    /// <summary>Empty cells in grow zones that have a crop chosen; the guy stands on the cell to sow.</summary>
    static IEnumerable<Vector2I> SowableCells()
    {
        foreach (var zone in Game.Map.GrowZones.Zones)
        {
            if (zone.Crop == null) continue; // nothing chosen to sow
            foreach (var cell in zone.Cells)
                if (!Game.Map.HasPlant(cell)) yield return cell; // skip already sown / growing
        }
    }
}
