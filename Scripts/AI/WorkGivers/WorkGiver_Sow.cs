using Godot;

/// <summary>Finds the nearest reachable empty cell in a grow zone and returns a sow job.</summary>
public class WorkGiver_Sow : WorkGiver
{
    public override Job TryGiveJob(Guy guy)
    {
        var reachable = Game.Pathing.ReachableCells(guy.Cell);

        Vector2I best = default;
        int bestDist = int.MaxValue;
        foreach (var zone in Game.Map.GrowZones.Zones)
        {
            if (zone.Crop == null) continue; // nothing chosen to sow
            foreach (var cell in zone.Cells)
            {
                if (Game.Map.HasPlant(cell)) continue; // already sown / growing
                if (!reachable.Contains(cell)) continue; // guy stands on the cell to sow
                if (!Game.Map.Reservations.AvailableCell(cell, guy)) continue;
                int dist = Grid.DistanceSquared(guy.Cell, cell);
                if (dist < bestDist) { bestDist = dist; best = cell; }
            }
        }
        if (bestDist == int.MaxValue) return null;

        return new Job { Type = JobType.Sow, TargetCell = best, ClaimsCell = true };
    }
}