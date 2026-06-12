using System.Linq;
using Godot;

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

        int room = Game.Map.Stockpiles.TotalRoomFor(item.Def);
        // no room, no pickup
        if (room == 0) return null;

        // only take what'll fit
        int amount = Mathf.Min(item.Count, room);
        return new Job { Type = JobType.Haul, TargetCell = item.Cell, TargetItem = item, Count = amount };
    }
}