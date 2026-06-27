using System.Collections.Generic;
using Godot;

/// <summary>Finds the nearest reachable plant to harvest, either one the player designated,
/// or a mature crop sitting in a grow zone,  and returns a harvest job.</summary>
public class WorkGiver_Harvest : WorkGiver
{
    public override Job TryGiveJob(Guy guy)
    {
        var reachable = Game.Pathing.ReachableCells(guy.Cell);

        bool CanReach(Vector2I cell)
        {
            foreach (var d in Grid.Adjacent8)
            {
                if (reachable.Contains(cell + d)) return true;
            }
            return false;
        }

        Vector2I best = default;
        int bestDist = int.MaxValue;
        foreach (var cell in HarvestableCells())
        {
            var plant = Game.Map.PlantAt(cell);
            if (plant == null || !plant.IsHarvestable) continue;
            if (!CanReach(cell) || !Game.Map.Reservations.AvailableCell(cell, guy)) continue;
            int dist = Grid.DistanceSquared(guy.Cell, cell);
            if (dist < bestDist) { bestDist = dist; best = cell; }
        }
        if (bestDist == int.MaxValue) return null;

        return new Job { Type = JobType.Harvest, TargetCell = best, ClaimsCell = true };
    }

    /// <summary>Cells worth checking: player harvest designations, plus every growzone cell</summary>
    static IEnumerable<Vector2I> HarvestableCells()
    {
        foreach (var cell in Game.Map.Designations.CellsOfType(DesignationType.Harvest))
            yield return cell;

        foreach (var zone in Game.Map.GrowZones.Zones)
            foreach (var cell in zone.Cells)
                yield return cell;
    }
}