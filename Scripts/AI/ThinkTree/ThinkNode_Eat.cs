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

        var food = Game.Map.LooseItems
            .Where(i => i.Def.IsFood)
            .OrderBy(i => guy.Cell.DistanceTo(i.Cell))
            .FirstOrDefault(i => Game.Pathing.IsReachable(guy.Cell, i.Cell));
        if (food == null) return null;

        return new Job { Type = JobType.Eat, TargetCell = food.Cell, TargetItem = food };
    }
}