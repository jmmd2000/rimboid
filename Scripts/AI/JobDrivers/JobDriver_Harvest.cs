using System.Collections.Generic;
using Godot;

/// <summary>
/// Job driver for harvesting a designated plant.
/// Tasks: walk beside it -> harvest over time -> finish (drop yield, remove plant, clear designation)
/// </summary>
public class JobDriver_Harvest : JobDriver
{
    float _workDone;

    /// <summary>A free cell beside the plant to drop the yield onto, so it isn't hidden under the
    /// plant's sprite. Prefers the cell below, falls back to the plant's own cell if nothing's free.</summary>
    static Vector2I DropCell(Vector2I plantCell)
    {
        bool Free(Vector2I c) =>
            Game.Map.InBounds(c) && Game.Map.Terrain[c.X, c.Y].Walkable
            && !Game.Map.BlocksMovementAt(c) && !Game.Map.HasPlant(c);

        if (Free(plantCell + Vector2I.Down)) return plantCell + Vector2I.Down;

        foreach (var d in Grid.Adjacent8)
            if (Free(plantCell + d)) return plantCell + d;

        // nowhere free, fall back to the plant's cell
        return plantCell;
    }

    protected override IEnumerable<Task> MakeTasks()
    {
        var plant = Game.Map.PlantAt(job.TargetCell);
        if (plant == null) yield break;

        // walk to the cell beside the plant
        yield return WalkTo(
            () => Game.Pathing.NearestReachableWorkCell(job.TargetCell, guy.Cell),
            failIf: () => !Game.Map.Designations.Has(DesignationType.Harvest, job.TargetCell)
        );

        // harvest over time
        yield return new Task
        {
            OnStart = () => _workDone = 0,
            OnTick = () => _workDone += 1f,
            IsComplete = () => _workDone >= plant.Def.WorkToHarvest,
            FailOn = () => !Game.Map.Designations.Has(DesignationType.Harvest, job.TargetCell),
        };

        // when done, drop yield, remove the plant, clear designation, reopen the cell
        yield return new Task
        {
            OnStart = () =>
            {
                if (plant.Def.HarvestItem != null)
                {
                    var dropCell = DropCell(job.TargetCell);
                    var (item, isNew, _) = Game.Map.SpawnItem(plant.Def.HarvestItem, dropCell, plant.Def.HarvestYield);
                    if (isNew) Game.Views.SpawnItemView(item);
                }

                if (plant.Def.RegrowDays > 0)
                {
                    //regrowing, keep the plant and its designation
                    plant.GrowthStartTick = GameTime.Ticks;
                    plant.MatureAtTick = GameTime.Ticks + (long)(plant.Def.RegrowDays * GameTime.TicksPerDay);
                }
                else
                {
                    Game.Map.RemovePlant(plant);
                    Game.Views.RemovePlantView(plant);
                    Game.Pathing.RefreshCell(Game.Map, job.TargetCell);
                }

                Game.Map.Designations.Remove(DesignationType.Harvest, job.TargetCell);
                Game.MapView.ClearDesignation(job.TargetCell);
            },
            IsComplete = () => true,
        };
    }
}