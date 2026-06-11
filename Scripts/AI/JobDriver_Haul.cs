using System.Collections.Generic;
using Godot;

/// <summary>
/// Job driver for hauling a loose item to a stockpile cell.
/// Tasks: walk to item -> pick up -> walk to stockpile -> drop.
/// </summary>
public class JobDriver_Haul : JobDriver
{
    bool _pathFailed;

    /// <summary>Yields the four haul tasks in order.</summary>
    /// <returns>Sequence of tasks for the job driver to execute.</returns>
    protected override IEnumerable<Task> MakeTasks()
    {
        // walk to the target
        yield return new Task
        {
            OnStart = () =>
            {
                if (guy.Cell == job.TargetCell) return;
                var path = Game.Pathing.GetPath(guy.Cell, job.TargetCell);
                if (path == null || path.Length < 2) { _pathFailed = true; return; }
                guy.StartPath(path);
            },
            OnTick = () => guy.MoveAlongPath(),
            IsComplete = () => guy.AtPathEnd,
            FailOn = () => _pathFailed,
        };

        // pick up
        yield return new Task
        {
            OnStart = () =>
            {
                guy.Carrying = job.TargetItem;
                Game.Map.LooseItems.Remove(job.TargetItem);
                Game.Main.RemoveItemView(job.TargetItem);
            },
            IsComplete = () => true,
        };

        // walk to stockpile cell
        yield return new Task
        {
            OnStart = () =>
            {
                if (guy.Cell == job.DestinationCell) return;
                var path = Game.Pathing.GetPath(guy.Cell, job.DestinationCell);
                if (path == null || path.Length < 2) { _pathFailed = true; return; }
                guy.StartPath(path);
            },
            OnTick = () => guy.MoveAlongPath(),
            IsComplete = () => guy.AtPathEnd,
            FailOn = () => _pathFailed,
        };

        // drop item
        yield return new Task
        {
            OnStart = () =>
            {
                Game.Map.SpawnItem(guy.Carrying.Def, job.DestinationCell, guy.Carrying.Count);
                var dropped = Game.Map.ItemAt(job.DestinationCell, guy.Carrying.Def);
                Game.Main.SpawnItemView(dropped);
                guy.Carrying = null;
            },
            IsComplete = () => true,
        };
    }
}