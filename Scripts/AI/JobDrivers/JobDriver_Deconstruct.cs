using System.Collections.Generic;
using Godot;

/// <summary>
/// Tears down a placed building.
/// Tasks: walk adjacent -> deconstruction work over time -> break it down
/// (free the cells, refund its full material cost, clear the designation, reopen pathing).
/// </summary>
public class JobDriver_Deconstruct : JobDriver
{
    float _workDone;

    protected override IEnumerable<Task> MakeTasks()
    {
        var building = Game.Map.BuildingAt(job.TargetCell);
        if (building == null) yield break; // already gone

        bool Gone() => Game.Map.BuildingAt(job.TargetCell) == null
                       || !Game.Map.Designations.Has(DesignationType.Deconstruct, job.TargetCell);

        // walk to a cell beside the building
        yield return WalkTo(
            () => Game.Pathing.NearestReachableCardinal(job.TargetCell, guy.Cell),
            failIf: Gone
        );

        // deconstruction work over time
        yield return new Task
        {
            OnStart = () => _workDone = 0,
            OnTick = () => _workDone += SkilledWork(SkillDefOf.Construction),
            IsComplete = () => _workDone >= building.Def.WorkToBuild,
            FailOn = Gone,
        };

        // break it down: free the cells first, then refund the full material cost onto the cleared origin
        yield return new Task
        {
            OnStart = () =>
            {
                var def = building.Def;
                Game.Map.RemoveBuilding(building);
                Game.Map.Designations.Remove(DesignationType.Deconstruct, job.TargetCell);
                foreach (var c in building.OccupiedCells) Game.Pathing.RefreshCell(Game.Map, c);

                int refund = def.MaterialCost;
                if (def.Materials != null && refund > 0)
                    Game.Map.DropItems(def.Materials, building.Cell, refund);
            },
            IsComplete = () => true,
        };
    }
}