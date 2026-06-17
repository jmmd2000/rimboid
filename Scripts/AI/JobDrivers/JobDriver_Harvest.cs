using System.Collections.Generic;

/// <summary>
/// Job driver for harvesting a designated plant.
/// Tasks: walk beside it -> harvest over time -> finish (drop yield, remove plant, clear designation)
/// </summary>
public class JobDriver_Harvest : JobDriver
{
    float _workDone;

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
                    var (item, isNew, _) = Game.Map.SpawnItem(plant.Def.HarvestItem, job.TargetCell, plant.Def.HarvestYield);
                    if (isNew) Game.Views.SpawnItemView(item);
                }

                Game.Map.RemovePlant(plant);
                Game.Views.RemovePlantView(plant);
                Game.Map.Designations.Remove(DesignationType.Harvest, job.TargetCell);
                Game.Pathing.RefreshCell(Game.Map, job.TargetCell);
                Game.MapView.ClearDesignation(job.TargetCell);
            },
            IsComplete = () => true,
        };
    }
}