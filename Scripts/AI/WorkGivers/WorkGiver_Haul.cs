using System.Linq;

/// <summary>Finds the nearest loose item outside any stockpile and returns a hauling job.</summary>
public class WorkGiver_Haul : WorkGiver
{
    public override Job TryGiveJob(Guy guy)
    {
        var item = Game.Map.LooseItems
        .Where(i => !Game.Map.Stockpiles.IsInStockpile(i))
        .OrderBy(i => guy.Cell.DistanceTo(i.Cell))
        .FirstOrDefault();
        if (item == null) return null;

        var dest = Game.Map.Stockpiles.BestCellFor(item.Def);
        if (dest == null) return null;

        return new Job { Type = JobType.Haul, TargetCell = item.Cell, TargetItem = item, DestinationCell = dest.Value };
    }
}