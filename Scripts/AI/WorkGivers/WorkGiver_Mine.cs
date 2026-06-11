using System.Linq;

/// <summary>Finds the nearest mine designation and returns a mining job.</summary>
public class WorkGiver_Mine : WorkGiver
{
    public override Job TryGiveJob(Guy guy)
    {
        var cells = Game.Map.Designations.CellsOfType(DesignationType.Mine);
        if (!cells.Any()) return null;

        var target = cells.OrderBy(c => guy.Cell.DistanceTo(c)).First();
        return new Job { Type = JobType.Mine, TargetCell = target };
    }
}