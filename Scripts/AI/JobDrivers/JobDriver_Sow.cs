using System.Collections.Generic;
using Godot;

/// <summary>
/// Job driver for sowing a crop on an empty grow-zone cell.
/// Tasks: walk onto the cell -> sow over time -> finish (spawn the crop at growth stage 0)
/// </summary>
public class JobDriver_Sow : JobDriver
{
    float _workDone;

    protected override IEnumerable<Task> MakeTasks()
    {
        var zone = Game.Map.GrowZones.ZoneAt(job.TargetCell);
        if (zone?.Crop == null) yield break;
        var crop = zone.Crop;

        // the sow job is void if the cell got sown by someone else or the zone was removed
        bool OrderStands() =>
            !Game.Map.HasPlant(job.TargetCell) && Game.Map.GrowZones.IsGrowZoneCell(job.TargetCell);

        // walk onto the empty cell
        yield return WalkTo(job.TargetCell, failIf: () => !OrderStands());

        // sow over time
        yield return new Task
        {
            OnStart = () => _workDone = 0,
            OnTick = () => _workDone += SkilledWork(SkillDefOf.Farming),
            IsComplete = () => _workDone >= crop.WorkToSow,
            FailOn = () => !OrderStands(),
        };

        // spawn the crop at stage 0 (sown: true sets its grow timing)
        yield return new Task
        {
            OnStart = () => Game.Map.SpawnPlant(crop, job.TargetCell, sown: true),
            IsComplete = () => true,
        };
    }
}