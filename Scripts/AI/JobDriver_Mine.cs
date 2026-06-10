using System.Collections.Generic;
using System.Linq;
using Godot;

public class JobDriver_Mine : JobDriver
{
    float _workDone;
    bool _pathFailed;

    protected override IEnumerable<Task> MakeTasks()
    {
        // walk to the target
        yield return new Task
        {
            OnStart = () =>
            {
                var adjacent = Game.Pathing.NearestWalkableNeighbour(job.TargetCell, guy.Cell);
                if (adjacent == null) { _pathFailed = true; return; }
                var path = Game.Pathing.GetPath(guy.Cell, adjacent.Value);
                if (path == null || path.Length < 2) { _pathFailed = true; return; }
                guy.StartPath(path);
            },
            OnTick = () => guy.MoveAlongPath(),
            IsComplete = () => guy.AtPathEnd,
            FailOn = () => _pathFailed,
        };

        // mine over time
        yield return new Task
        {
            OnStart = () => _workDone = 0,
            OnTick = () => _workDone += 1f,
            IsComplete = () => _workDone >= 80f,
            FailOn = () => !Game.Map.Designations.Has(DesignationType.Mine, job.TargetCell),
        };

        // when done, change terrain, remove designation
        yield return new Task
        {
            OnStart = () =>
            {
                Game.Map.SpawnItem(ItemDefOf.Stone, job.TargetCell, 2);
                var item = Game.Map.LooseItems.Last();
                Game.Main.SpawnItemView(item);
                Game.Map.Terrain[job.TargetCell.X, job.TargetCell.Y] = TerrainDefOf.Dirt;
                Game.Map.Designations.Remove(DesignationType.Mine, job.TargetCell);
                Game.Pathing.RefreshCell(Game.Map, job.TargetCell);
                Game.MapView.PaintCell(job.TargetCell);
                Game.MapView.ClearDesignation(job.TargetCell);
            },
            IsComplete = () => true,
        };
    }
}