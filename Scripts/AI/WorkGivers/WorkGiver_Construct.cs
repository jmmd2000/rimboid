using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>
/// Finds construction frames needing materials and returns a haul-to-frame job that gathers
/// enough nearby stuff (across several piles if needed) in one trip. Frames already stocked are built.
/// </summary>
public class WorkGiver_Construct : WorkGiver
{
    public override Job TryGiveJob(Guy guy)
    {
        if (Game.Map.Frames.Count == 0) return null;

        var reachable = Game.Pathing.ReachableCells(guy.Cell);
        bool CanReach(Frame frame) => Grid.Adjacent8.Any(d => reachable.Contains(frame.Cell + d));

        bool Workable(Frame f) =>
            Game.Map.Reservations.AvailableCell(f.Cell, guy) &&
            CanReach(f) &&
            (!f.MaterialsComplete || Game.Pathing.NearestSafeWorkCell(f.Cell, guy.Cell) != null);

        // nearest workable frame in one pass, no OrderBy sort/allocation every think. The distance guard
        // skips the (sometimes pathfinding) Workable check on any frame that can't beat the best so far.
        Frame frame = null;
        int bestDist = int.MaxValue;
        foreach (var f in Game.Map.Frames)
        {
            int dist = Grid.DistanceSquared(guy.Cell, f.Cell);
            if (dist >= bestDist) continue;
            if (Workable(f)) { bestDist = dist; frame = f; }
        }
        if (frame == null) return null;

        if (frame.MaterialsComplete)
        {
            return new Job { Type = JobType.Build, TargetCell = frame.Cell, ClaimsCell = true };
        }

        // gather reachable, unreserved piles of the material up to what the frame still needs
        int needed = frame.Def.MaterialCost - frame.MaterialsDelivered;
        var picks = new List<Item>();
        int total = 0;
        foreach (var item in Game.Map.LooseItems)
        {
            if (total >= needed) break;
            if (item.Def != frame.Def.Materials) continue;
            if (!reachable.Contains(item.Cell)) continue;
            if (!Game.Map.Reservations.AvailableItem(item, guy)) continue;
            picks.Add(item);
            total += item.Count;
        }
        if (picks.Count == 0) return null;

        int amount = Mathf.Min(needed, total);
        return new Job { Type = JobType.HaulToFrame, TargetCell = frame.Cell, ReservedItems = picks, Count = amount, ClaimsCell = true };
    }
}