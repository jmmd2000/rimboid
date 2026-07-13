using Godot;

/// <summary>Finds the nearest reachable mine designation and returns a mining job.</summary>
public class WorkGiver_Mine : WorkGiver
{
    public override Job TryGiveJob(Guy guy)
    {
        var reachable = Game.Pathing.ReachableCells(guy.Cell);

        // mineable if the guy can stand on any of the 4 cardinal neighbours to reach it
        bool CanReach(Vector2I cell)
        {
            foreach (var d in Grid.Cardinal4)
            {
                if (reachable.Contains(cell + d)) return true;
            }
            return false;
        }

        Vector2I best = default;
        int bestDist = int.MaxValue;
        foreach (var cell in Game.Map.Designations.CellsOfType(DesignationType.Mine))
        {
            if (!CanReach(cell) || !Game.Map.Reservations.AvailableCell(cell, guy)) continue;
            int dist = Grid.DistanceSquared(guy.Cell, cell);
            if (dist < bestDist) { bestDist = dist; best = cell; }
        }
        if (bestDist == int.MaxValue) return null;

        return new Job { Type = JobType.Mine, TargetCell = best, ClaimsCell = true };
    }
}