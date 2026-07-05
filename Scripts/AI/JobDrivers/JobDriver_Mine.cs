using System.Collections.Generic;
using Godot;

/// <summary>
/// Job driver for mining a designated mineable cell.
/// Tasks: Walk adjacent -> mine over time -> finish (drop the terrain's yield, swap terrain, clear designation)
/// </summary>
public class JobDriver_Mine : JobDriver
{
    float _workDone;

    /// <summary>
    /// Yields the three tasks: walk to adjacent cell, mine, then finish up.
    /// </summary>
    /// <returns>Sequence of tasks for the job driver to execute.</returns>
    protected override IEnumerable<Task> MakeTasks()
    {
        var terrain = Game.Map.Terrain[job.TargetCell.X, job.TargetCell.Y];

        // walk to a cell adjacent to the target
        yield return WalkTo(
            () => Game.Pathing.NearestReachableCardinal(job.TargetCell, guy.Cell),
            failIf: () => !Game.Map.Designations.Has(DesignationType.Mine, job.TargetCell)
        );

        // mine over time
        yield return new Task
        {
            OnStart = () => _workDone = 0,
            OnTick = () => _workDone += SkilledWork(SkillDefOf.Mining),
            IsComplete = () => _workDone >= terrain.WorkToMine,
            FailOn = () => !Game.Map.Designations.Has(DesignationType.Mine, job.TargetCell),
        };

        // when done, change terrain, remove designation
        yield return new Task
        {
            OnStart = () =>
            {
                if (terrain.MinedItem != null)
                {
                    var (item, isNew, _) = Game.Map.SpawnItem(terrain.MinedItem, job.TargetCell, terrain.MineYield);
                    if (isNew) Game.Views.SpawnItemView(item);
                }

                Game.Map.Terrain[job.TargetCell.X, job.TargetCell.Y] = terrain.TerrainAfterMined ?? TerrainDefOf.Dirt;
                Game.Map.Designations.Remove(DesignationType.Mine, job.TargetCell);
                Game.Pathing.RefreshCell(Game.Map, job.TargetCell);
                Game.MapView.PaintCell(job.TargetCell);
                Game.MapView.ClearDesignation(job.TargetCell);
            },
            IsComplete = () => true,
        };
    }
}