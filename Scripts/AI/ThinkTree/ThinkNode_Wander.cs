using Godot;

/// <summary>Fallback think node. Sends the guy to a random nearby reachable cell.</summary>
public class ThinkNode_Wander : IThinkNode
{
    const int Radius = 5;
    const int Attempts = 10;

    static readonly System.Random _rng = new();

    public Job TryGiveJob(Guy guy)
    {
        var reachable = Game.Pathing.ReachableCells(guy.Cell);
        for (int i = 0; i < Attempts; i++)
        {
            int dx = _rng.Next(-Radius, Radius + 1);
            int dy = _rng.Next(-Radius, Radius + 1);
            var cell = new Vector2I(guy.Cell.X + dx, guy.Cell.Y + dy);

            if (cell == guy.Cell) continue;
            if (!reachable.Contains(cell)) continue;

            return new Job { Type = JobType.Wander, TargetCell = cell };
        }
        return null; // no reachable cell found this tick, idle and retry next time.
    }
}