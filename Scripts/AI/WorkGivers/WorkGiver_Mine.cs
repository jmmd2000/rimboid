using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>Finds the nearest reachable mine designation and returns a mining job.</summary>
public class WorkGiver_Mine : WorkGiver
{
    public override Job TryGiveJob(Guy guy)
    {
        var reachable = Game.Pathing.ReachableCells(guy.Cell);

        // mineable if the guy can stand on any of the 8 neighbours to reach it
        bool CanReach(Vector2I cell) => Grid.Adjacent8.Any(d => reachable.Contains(cell + d));

        List<Vector2I> candidates = Game.Map.Designations.CellsOfType(DesignationType.Mine)
            .Where(CanReach)
            .Where(c => Game.Map.Reservations.AvailableCell(c, guy))
            .OrderBy(c => guy.Cell.DistanceTo(c))
            .ToList();

        if (candidates.Count == 0) return null;

        return new Job { Type = JobType.Mine, TargetCell = candidates[0], ClaimsCell = true };
    }
}