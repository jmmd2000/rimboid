using System.Collections.Generic;
using Godot;

/// <summary>
/// Job driver for harvesting a designated plant.
/// Tasks: walk beside it -> harvest over time -> finish (drop yield, remove plant, clear designation)
/// </summary>
public class JobDriver_Harvest : JobDriver
{
    float _workDone;

    /// <summary>The harvest order is still valid while the plant is ripe and the cell is either
    /// designated for harvest or part of a grow zone. Cancelling either voids the job.</summary>
    bool HarvestOrderStands()
    {
        var plant = Game.Map.PlantAt(job.TargetCell);
        return plant != null && plant.IsHarvestable
            && (Game.Map.Designations.Has(DesignationType.Harvest, job.TargetCell)
                || Game.Map.GrowZones.IsGrowZoneCell(job.TargetCell));
    }

    protected override IEnumerable<Task> MakeTasks()
    {
        var plant = Game.Map.PlantAt(job.TargetCell);
        if (plant == null) yield break;

        // walk to the cell beside the plant
        yield return WalkTo(
            () => Game.Pathing.NearestReachableWorkCell(job.TargetCell, guy.Cell),
            failIf: () => !HarvestOrderStands()
        );

        // harvest over time
        yield return new Task
        {
            OnStart = () => _workDone = 0,
            OnTick = () => _workDone += SkilledWork(SkillDefOf.Farming),
            IsComplete = () => _workDone >= plant.Def.WorkToHarvest,
            FailOn = () => !HarvestOrderStands(),
        };

        // when done, drop yield, remove the plant, clear designation, reopen the cell
        yield return new Task
        {
            OnStart = () =>
            {
                if (plant.Def.HarvestItem != null)
                {
                    var dropCell = Game.Map.FreeDropCell(job.TargetCell);
                    Game.Map.DropItems(plant.Def.HarvestItem, dropCell, plant.Def.HarvestYield);
                }

                if (plant.Def.RegrowDays > 0)
                {
                    //regrowing, keep the plant and its designation
                    plant.StartGrowing(plant.Def.RegrowDays);
                }
                else
                {
                    Game.Map.RemovePlant(plant);
                    Game.Pathing.RefreshCell(Game.Map, job.TargetCell);
                }

                Game.Map.Designations.Remove(DesignationType.Harvest, job.TargetCell);
            },
            IsComplete = () => true,
        };
    }
}