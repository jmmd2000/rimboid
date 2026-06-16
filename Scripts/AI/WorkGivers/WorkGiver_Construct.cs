using System.Linq;
using Godot;

/// <summary>
/// Finds construction frames needing materials and returns a haul-to-frame job that
/// fetches the nearest matching stuff. Building frames that are already stocked comes later.
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

        // nearest frame with a walkable neighbour
        var frame = Game.Map.Frames
            .OrderBy(f => guy.Cell.DistanceTo(f.Cell))
            .FirstOrDefault(Workable);
        if (frame == null) return null;

        if (frame.MaterialsComplete)
        {
            return new Job { Type = JobType.Build, TargetCell = frame.Cell, ClaimsCell = true };
        }

        Item materials = null;
        int bestDist = int.MaxValue;
        foreach (var i in Game.Map.LooseItems)
        {
            if (i.Def != frame.Def.Materials) continue;
            if (!reachable.Contains(i.Cell)) continue;
            if (!Game.Map.Reservations.AvailableItem(i, guy)) continue;
            int dist = Grid.DistanceSquared(guy.Cell, i.Cell);
            if (dist < bestDist) { bestDist = dist; materials = i; }
        }
        if (materials == null) return null;

        int needed = frame.Def.MaterialCost - frame.MaterialsDelivered;
        int amount = Mathf.Min(materials.Count, needed);
        return new Job { Type = JobType.HaulToFrame, TargetCell = frame.Cell, TargetItem = materials, Count = amount, ClaimsCell = true };
    }
}