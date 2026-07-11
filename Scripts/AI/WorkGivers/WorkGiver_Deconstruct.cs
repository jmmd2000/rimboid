using Godot;

/// <summary>Finds the nearest reachable, unreserved deconstruct designation and returns a deconstruct job.</summary>
public class WorkGiver_Deconstruct : WorkGiver
{
    public override Job TryGiveJob(Guy guy)
    {
        var reachable = Game.Pathing.ReachableCells(guy.Cell);

        // workable if the guy can stand on any cardinal neighbour to reach it
        bool CanReach(Vector2I cell)
        {
            foreach (var d in Grid.Cardinal4)
                if (reachable.Contains(cell + d)) return true;
            return false;
        }

        Vector2I best = default;
        int bestDist = int.MaxValue;
        foreach (var cell in Game.Map.Designations.CellsOfType(DesignationType.Deconstruct))
        {
            if (!CanReach(cell) || !Game.Map.Reservations.AvailableCell(cell, guy)) continue;
            int dist = Grid.DistanceSquared(guy.Cell, cell);
            if (dist < bestDist) { bestDist = dist; best = cell; }
        }
        if (bestDist == int.MaxValue) return null;

        return new Job { Type = JobType.Deconstruct, TargetCell = best, ClaimsCell = true };
    }
}