using System.Collections.Generic;
using Godot;

/// <summary>
/// Hauls one load of material to a construction frame.
/// Tasks: walk to the stuff -> pick it up -> walk adjacent to the frame -> deposit into it.
/// Fails (dropping the load) if the frame is cancelled mid-haul.
/// </summary>
public class JobDriver_HaulToFrame : JobDriver
{
    bool _pathFailed;

    protected override IEnumerable<Task> MakeTasks()
    {
        // walk to the material pile
        yield return new Task
        {
            OnStart = () =>
            {
                if (guy.Cell == job.TargetItem.Cell) return;
                var path = Game.Pathing.GetPath(guy.Cell, job.TargetItem.Cell);
                if (path == null || path.Length < 2) { _pathFailed = true; return; }
                guy.StartPath(path);
            },
            OnTick = () => guy.MoveAlongPath(),
            IsComplete = () => guy.AtPathEnd,
            FailOn = () => _pathFailed || !Game.Map.HasFrame(job.TargetCell),
        };

        // pick up the load (only what the frame still needs)
        yield return new Task
        {
            OnStart = () =>
            {
                var src = job.TargetItem;
                if (job.Count >= src.Count)
                {
                    guy.Carrying = src;
                    Game.Map.RemoveItem(src);
                    Game.Main.RemoveItemView(src);
                }
                else
                {
                    src.Count -= job.Count;
                    guy.Carrying = new Item { Def = src.Def, Count = job.Count };
                }
            },
            IsComplete = () => true,
            FailOn = () => !Game.Map.HasFrame(job.TargetCell),
        };

        // walk to a cell adjacent to the frame
        yield return new Task
        {
            OnStart = () =>
            {
                var adjacent = Game.Pathing.NearestReachableWorkCell(job.TargetCell, guy.Cell);
                if (adjacent == null) { _pathFailed = true; return; }
                // already adjacent
                if (guy.Cell == adjacent.Value) return;
                var path = Game.Pathing.GetPath(guy.Cell, adjacent.Value);
                if (path == null || path.Length < 2) { _pathFailed = true; return; }
                guy.StartPath(path);
            },
            OnTick = () => guy.MoveAlongPath(),
            IsComplete = () => guy.AtPathEnd,
            FailOn = () => _pathFailed || !Game.Map.HasFrame(job.TargetCell),
        };

        // deposit into the frame
        yield return new Task
        {
            OnStart = () =>
            {
                var frame = Game.Map.FrameAt(job.TargetCell);
                if (frame == null || guy.Carrying == null) return;
                frame.MaterialsDelivered += guy.Carrying.Count;
                guy.Carrying = null;
            },
            IsComplete = () => true,
            FailOn = () => !Game.Map.HasFrame(job.TargetCell),
        };
    }
}