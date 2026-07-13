using System.Collections.Generic;
using Godot;

/// <summary>Scans the world for one category of work and returns a job if available.</summary>
public abstract class WorkGiver
{
    public abstract Job TryGiveJob(Guy guy);

    /// <summary>How a guy reaches a target cell: standing on it, or on a neighbour.</summary>
    protected enum Reach { OnCell, Cardinal, Adjacent8 }

    /// <summary>The nearest candidate cell the guy can reach and reserve, or null. Shared by the cell-based
    /// work givers so each doesn't hand-roll the same nearest-tracking scan.</summary>
    protected static Vector2I? NearestReachableCell(IEnumerable<Vector2I> candidates, Guy guy,
        HashSet<Vector2I> reachable, Reach reach)
    {
        Vector2I? best = null;
        int bestDist = int.MaxValue;
        foreach (var cell in candidates)
        {
            if (!CanReach(cell, reachable, reach)) continue;
            if (!Game.Map.Reservations.AvailableCell(cell, guy)) continue;
            int dist = Grid.DistanceSquared(guy.Cell, cell);
            if (dist < bestDist) { bestDist = dist; best = cell; }
        }
        return best;
    }

    /// <summary>True if the cell is workable under the reach rule: the guy stands on it, or on a cardinal
    /// (orthogonal) neighbour, or on any of the 8 surrounding cells.</summary>
    static bool CanReach(Vector2I cell, HashSet<Vector2I> reachable, Reach reach)
    {
        if (reach == Reach.OnCell) return reachable.Contains(cell);

        var dirs = reach == Reach.Cardinal ? Grid.Cardinal4 : Grid.Adjacent8;
        foreach (var d in dirs)
            if (reachable.Contains(cell + d)) return true;
        return false;
    }
}
