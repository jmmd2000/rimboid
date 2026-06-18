using System.Collections.Generic;
using Godot;

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
                    var dropCell = Game.Map.FreeDropCell(job.TargetCell);
                    Game.Views.DropItems(plant.Def.HarvestItem, dropCell, plant.Def.HarvestYield);
                }

                if (plant.Def.RegrowDays > 0)
                {
                    //regrowing, keep the plant and its designation
                    plant.StartGrowing(plant.Def.RegrowDays);
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