using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>Finds the nearest reachable mine designation and returns a mining job.</summary>
public class WorkGiver_Mine : WorkGiver
{
    public override Job TryGiveJob(Guy guy)
    {
        List<Vector2I> candidates = Game.Map.Designations.CellsOfType(DesignationType.Mine)
            .Where(c => Game.Pathing.NearestWalkableNeighbour(c, guy.Cell) != null)
            .OrderBy(c => guy.Cell.DistanceTo(c))
            .ToList();

        if (candidates.Count == 0) return null;

        return new Job { Type = JobType.Mine, TargetCell = candidates[0] };
    }
}