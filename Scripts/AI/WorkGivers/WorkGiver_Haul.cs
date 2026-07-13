using System.Linq;
using Godot;

/// <summary>Finds the nearest reachable loose item outside any stockpile and returns a hauling job.</summary>
public class WorkGiver_Haul : WorkGiver
{

    public override WorkType Work => WorkType.Haul;

    public override Job TryGiveJob(Guy guy)
    {
        var reachable = Game.Pathing.ReachableCells(guy.Cell);

        Item best = null;
        int bestDist = int.MaxValue;
        foreach (var i in Game.Map.LooseItems)
        {
            if (Game.Map.Stockpiles.IsInStockpile(i)) continue;
            if (!reachable.Contains(i.Cell)) continue;
            if (!Game.Map.Reservations.AvailableItem(i, guy)) continue;
            int dist = Grid.DistanceSquared(guy.Cell, i.Cell);
            if (dist < bestDist) { bestDist = dist; best = i; }
        }
        if (best == null) return null;

        int room = Game.Map.Stockpiles.TotalRoomFor(best.Def);
        if (room == 0) return null;
        int amount = Mathf.Min(best.Count, room);
        return new Job { Type = JobType.Haul, TargetCell = best.Cell, TargetItem = best, Count = amount };
    }
}