using System.Linq;

/// <summary>Sends a hungry colonist to the nearest reachable food</summary>
public class ThinkNode_Eat : IThinkNode
{
    readonly bool _urgent;

    /// <summary>Creates an eat node.</summary>
    /// <param name="urgent">True for a critical tier.</param>
    public ThinkNode_Eat(bool urgent) => _urgent = urgent;

    public bool Interrupts => _urgent;

    public Job TryGiveJob(Guy guy)
    {
        float threshold = _urgent ? guy.Needs.Food.StarvingThreshold : guy.Needs.Food.HungryThreshold;
        if (guy.Needs.Food.Level >= threshold) return null;

        var reachable = Game.Pathing.ReachableCells(guy.Cell);

        Item best = null;
        int bestDist = int.MaxValue;
        foreach (var i in Game.Map.LooseItems)
        {
            if (!i.Def.IsFood) continue;
            if (!reachable.Contains(i.Cell)) continue;
            if (!Game.Map.Reservations.AvailableItem(i, guy)) continue;
            int dist = Grid.DistanceSquared(guy.Cell, i.Cell);
            if (dist < bestDist) { bestDist = dist; best = i; }
        }
        if (best == null) return null;

        return new Job { Type = JobType.Eat, TargetCell = best.Cell, TargetItem = best };
    }
}