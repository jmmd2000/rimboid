using Godot;

/// <summary>Sends a tired colonist to sleep when rest drops below the threshold: to a free bed when
/// not urgent, or dropping on the spot when urgent or no bed is reachable.</summary>
public class ThinkNode_Sleep : IThinkNode
{
    readonly bool _urgent;

    /// <summary>Creates a sleep node.</summary>
    /// <param name="urgent">True for a critical tier.</param>
    public ThinkNode_Sleep(bool urgent) => _urgent = urgent;

    public bool Interrupts => _urgent;

    public Job TryGiveJob(Guy guy)
    {
        float threshold = _urgent ? guy.Needs.Rest.CollapseThreshold : guy.Needs.Rest.TiredThreshold;
        bool tired = guy.Needs.Rest.Level < threshold;
        bool sleepHour = !_urgent && guy.Schedule.BlockNow() == ScheduleBlock.Sleep; // bed down at night even if not tired
        if (!tired && !sleepHour) return null;

        // when not about to collapse, make for a free bed; otherwise drop where standing
        if (!_urgent)
        {
            var bed = NearestFreeBed(guy);
            if (bed != null) return new Job { Type = JobType.Sleep, TargetCell = bed.Value, ClaimsCell = true };
        }
        return new Job { Type = JobType.Sleep };
    }

    /// <summary>The nearest reachable, unreserved bed cell, or null if there's none.</summary>
    static Vector2I? NearestFreeBed(Guy guy)
    {
        var reachable = Game.Pathing.ReachableCells(guy.Cell);

        Vector2I? best = null;
        int bestDist = int.MaxValue;
        foreach (var (_, building) in Game.Map.Buildings)
        {
            if (building.GetComponent<BuildingComponent_Bed>() == null) continue;

            // claim the whole bed by its origin, so a multi-cell bed can't be double-booked
            var cell = building.Cell;
            if (!reachable.Contains(cell) || !Game.Map.Reservations.AvailableCell(cell, guy)) continue;
            int dist = Grid.DistanceSquared(guy.Cell, cell);
            if (dist < bestDist) { bestDist = dist; best = cell; }
        }
        return best;
    }
}
