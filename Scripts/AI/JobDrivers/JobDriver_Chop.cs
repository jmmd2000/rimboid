using System.Collections.Generic;

/// <summary>
/// Job driver for chopping a designated tree.
/// Tasks: walk beside it -> chop over time -> finish (drop wood, fell the tree, clear designation)
/// </summary>
public class JobDriver_Chop : JobDriver
{
    float _workDone;

    protected override IEnumerable<Task> MakeTasks()
    {
        var plant = Game.Map.PlantAt(job.TargetCell);
        if (plant == null) yield break;

        yield return WalkTo(
            () => Game.Pathing.NearestReachableWorkCell(job.TargetCell, guy.Cell),
            failIf: () => !Game.Map.Designations.Has(DesignationType.Chop, job.TargetCell)
        );

        yield return new Task
        {
            OnStart = () => _workDone = 0,
            OnTick = () => _workDone += SkilledWork(SkillDefOf.Scavenging),
            IsComplete = () => _workDone >= plant.Def.WorkToHarvest,
            FailOn = () => !Game.Map.Designations.Has(DesignationType.Chop, job.TargetCell),
        };

        yield return new Task
        {
            OnStart = () =>
            {
                if (plant.Def.HarvestItem != null)
                {
                    var dropCell = Game.Map.FreeDropCell(job.TargetCell);
                    Game.Map.DropItems(plant.Def.HarvestItem, dropCell, plant.Def.HarvestYield);
                }

                Game.Map.RemovePlant(plant);
                if (plant.Def.Topples) Game.Views.ToppleAndRemovePlantView(plant);
                else Game.Views.RemovePlantView(plant);

                if (plant.Def.LeavesBehind != null)
                {
                    var stump = Game.Map.SpawnPlant(plant.Def.LeavesBehind, job.TargetCell);
                    stump.DrawWidth = plant.DrawWidth;
                    Game.Views.SpawnPlantView(stump);
                }

                Game.Map.Designations.Remove(DesignationType.Chop, job.TargetCell);
                Game.Pathing.RefreshCell(Game.Map, job.TargetCell);
                Game.MapView.ClearDesignation(job.TargetCell);
            },
            IsComplete = () => true,
        };
    }
}