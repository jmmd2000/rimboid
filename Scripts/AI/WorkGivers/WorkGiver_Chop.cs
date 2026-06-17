using Godot;

/// <summary>Finds the nearest reachable tree designated for chopping and returns a chop job.</summary>
public class WorkGiver_Chop : WorkGiver
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
        foreach (var cell in Game.Map.Designations.CellsOfType(DesignationType.Chop))
        {
            if (!CanReach(cell) || !Game.Map.Reservations.AvailableCell(cell, guy)) continue;
            int dist = Grid.DistanceSquared(guy.Cell, cell);
            if (dist < bestDist) { bestDist = dist; best = cell; }
        }
        if (bestDist == int.MaxValue) return null;

        return new Job { Type = JobType.Chop, TargetCell = best, ClaimsCell = true };
    }
}