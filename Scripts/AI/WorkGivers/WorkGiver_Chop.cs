/// <summary>Finds the nearest reachable tree designated for chopping and returns a chop job.</summary>
public class WorkGiver_Chop : WorkGiver
{

    public override WorkType Work => WorkType.Chop;

    public override Job TryGiveJob(Guy guy)
    {
        var reachable = Game.Pathing.ReachableCells(guy.Cell);
        var best = NearestReachableCell(Game.Map.Designations.CellsOfType(DesignationType.Chop), guy, reachable, Reach.Adjacent8);
        if (best == null) return null;

        return new Job { Type = JobType.Chop, TargetCell = best.Value, ClaimsCell = true };
    }
}
