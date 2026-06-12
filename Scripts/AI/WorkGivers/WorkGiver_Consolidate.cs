using System.Linq;

public class WorkGiver_Consolidate : WorkGiver
{
    public override Job TryGiveJob(Guy guy)
    {
        var byDef = Game.Map.LooseItems
            .Where(i => i.Count < i.Def.MaxStackSize && Game.Map.Stockpiles.IsInStockpile(i))
            .GroupBy(i => i.Def);

        foreach (var group in byDef)
        {
            var piles = group.ToList();
            // nothing to merge
            if (piles.Count < 2) continue;

            var source = piles.OrderBy(i => i.Count).First();
            int otherRoom = piles.Where(p => p != source).Sum(p => p.Def.MaxStackSize - p.Count);
            // if it wouldnt empty a cell if you moved it, skip
            if (source.Count > otherRoom) continue;
            if (!Game.Pathing.IsReachable(guy.Cell, source.Cell)) continue;

            return new Job { Type = JobType.Haul, TargetCell = source.Cell, TargetItem = source, Count = source.Count };
        }
        return null;
    }
}