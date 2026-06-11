using Godot;

/// <summary>Fallback think node. Sends the guy to a random nearby reachable cell.</summary>
public class ThinkNode_Wander : IThinkNode
{
    const int Radius = 5;
    const int Attempts = 10;

    static readonly System.Random _rng = new();

    public Job TryGiveJob(Guy guy)
    {
        for (int i = 0; i < Attempts; i++)
        {
            int dx = _rng.Next(-Radius, Radius + 1);
            int dy = _rng.Next(-Radius, Radius + 1);
            var cell = new Vector2I(guy.Cell.X + dx, guy.Cell.Y + dy);

            if (cell == guy.Cell) continue;
            if (cell.X < 0 || cell.X >= Game.Map.Width || cell.Y < 0 || cell.Y >= Game.Map.Height) continue;

            var path = Game.Pathing.GetPath(guy.Cell, cell);
            if (path == null || path.Length < 2) continue;

            return new Job { Type = JobType.Wander, TargetCell = cell };
        }
        return null; // no reachable cell found this tick, idle and retry next time.
    }
}