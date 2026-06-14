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
        bool Workable(Frame frame) => frame.MaterialsComplete
        ? Game.Pathing.NearestSafeWorkCell(frame.Cell, guy.Cell) != null
        : Game.Pathing.NearestReachableWorkCell(frame.Cell, guy.Cell) != null;

        // nearest frame with a walkable neighbour
        var frame = Game.Map.Frames
            .OrderBy(f => guy.Cell.DistanceTo(f.Cell))
            .FirstOrDefault(Workable);
        if (frame == null) return null;

        if (frame.MaterialsComplete)
        {
            return new Job { Type = JobType.Build, TargetCell = frame.Cell };
        }

        var materials = Game.Map.LooseItems
            .Where(i => i.Def == frame.Def.Materials && Game.Pathing.IsReachable(guy.Cell, i.Cell))
            .OrderBy(i => guy.Cell.DistanceTo(i.Cell))
            .FirstOrDefault();
        if (materials == null) return null;

        int needed = frame.Def.MaterialCost - frame.MaterialsDelivered;
        int amount = Mathf.Min(materials.Count, needed);
        return new Job { Type = JobType.HaulToFrame, TargetCell = frame.Cell, TargetItem = materials, Count = amount };
    }
}