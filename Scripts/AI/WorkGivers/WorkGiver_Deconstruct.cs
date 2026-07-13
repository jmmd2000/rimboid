/// <summary>Finds the nearest reachable, unreserved deconstruct designation and returns a deconstruct job.</summary>
public class WorkGiver_Deconstruct : WorkGiver
{
    public override Job TryGiveJob(Guy guy)
    {
        var reachable = Game.Pathing.ReachableCells(guy.Cell);
        // a guy can stand on any cardinal neighbour to reach it
        var best = NearestReachableCell(Game.Map.Designations.CellsOfType(DesignationType.Deconstruct), guy, reachable, Reach.Cardinal);
        if (best == null) return null;

        return new Job { Type = JobType.Deconstruct, TargetCell = best.Value, ClaimsCell = true };
    }
}
