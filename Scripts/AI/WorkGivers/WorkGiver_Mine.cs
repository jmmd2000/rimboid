/// <summary>Finds the nearest reachable mine designation and returns a mining job.</summary>
public class WorkGiver_Mine : WorkGiver
{
    public override Job TryGiveJob(Guy guy)
    {
        var reachable = Game.Pathing.ReachableCells(guy.Cell);
        // mineable if the guy can stand on any of the 4 cardinal neighbours
        var best = NearestReachableCell(Game.Map.Designations.CellsOfType(DesignationType.Mine), guy, reachable, Reach.Cardinal);
        if (best == null) return null;

        return new Job { Type = JobType.Mine, TargetCell = best.Value, ClaimsCell = true };
    }
}
