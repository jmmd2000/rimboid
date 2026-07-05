using System.Collections.Generic;
using Godot;

/// <summary>
/// Builds a stocked frame into a finished building.
/// Tasks: walk adjacent -> construction work over time -> raise the building
/// (spawn it, drop the frame, refresh pathing so the cell blocks movement).
/// </summary>
public class JobDriver_Build : JobDriver
{
    protected override IEnumerable<Task> MakeTasks()
    {
        bool FrameGone() => !Game.Map.HasFrame(job.TargetCell);

        // walk to a cell beside the frame
        yield return WalkTo(
            () => Game.Pathing.NearestSafeWorkCell(job.TargetCell, guy.Cell),
            failIf: FrameGone
        );

        // construction work over time
        yield return new Task
        {
            OnTick = () => { var f = Game.Map.FrameAt(job.TargetCell); if (f != null) f.WorkDone += SkilledWork(SkillDefOf.Construction); },
            IsComplete = () => { var f = Game.Map.FrameAt(job.TargetCell); return f == null || f.WorkComplete; },
            FailOn = FrameGone,
        };

        // a blocking building can't go up while a colonist stands on its footprint
        yield return new Task
        {
            IsComplete = () =>
            {
                var f = Game.Map.FrameAt(job.TargetCell);
                return f == null || !f.Def.BlocksMovement || !FootprintBlocked(f);
            },
            FailOn = FrameGone,
        };

        // frame -> finished building, across the whole footprint
        yield return new Task
        {
            OnStart = () =>
            {
                var frame = Game.Map.FrameAt(job.TargetCell);
                if (frame == null) return;

                var building = Game.Map.SpawnBuilding(frame.Def, frame.Cell, frame.Rotation);
                Game.Map.RemoveFrame(frame);
                Game.Views.RemoveFrameView(frame);
                Game.Views.SpawnBuildingView(building);
                foreach (var c in building.OccupiedCells) Game.Pathing.RefreshCell(Game.Map, c);
            },
            IsComplete = () => true,
        };
    }

    static bool FootprintBlocked(Frame frame)
    {
        foreach (var guy in Game.Map.Guys)
            foreach (var c in frame.OccupiedCells)
                if (guy.Cell == c) return true;
        return false;
    }
}