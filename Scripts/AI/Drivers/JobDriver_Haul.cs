using System.Collections.Generic;
using Godot;

/// <summary>
/// Job driver for hauling a loose item to a stockpile.
/// Walks to the item, picks up only what will fit, then deposits it across one or
/// more stockpile cells, splitting the stack so no pile exceeds its max size.
/// </summary>
public class JobDriver_Haul : JobDriver
{
    bool _pathFailed;

    /// <summary>Yields the walk, pick-up, and one-or-more deposit tasks.</summary>
    /// <returns>Sequence of tasks for the job driver to execute.</returns>
    protected override IEnumerable<Task> MakeTasks()
    {
        // walk to the target
        yield return new Task
        {
            OnStart = () =>
            {
                if (guy.Cell == job.TargetCell) return;
                var path = Game.Pathing.GetPath(guy.Cell, job.TargetCell);
                if (path == null || path.Length < 2) { _pathFailed = true; return; }
                guy.StartPath(path);
            },
            OnTick = () => guy.MoveAlongPath(),
            IsComplete = () => guy.AtPathEnd,
            FailOn = () => _pathFailed,
        };

        // pick up only as much as the stockpile can hold
        yield return new Task
        {
            OnStart = () =>
            {
                var src = job.TargetItem;
                if (job.Count >= src.Count)
                {
                    guy.Carrying = src;
                    Game.Map.LooseItems.Remove(src);
                    Game.Main.RemoveItemView(src);
                }
                else
                {
                    src.Count -= job.Count;
                    guy.Carrying = new Item { Def = src.Def, Count = job.Count };
                }

            },
            IsComplete = () => true,
        };

        // deposit, splitting across stacks until nothing's left to carry
        while (guy.Carrying != null)
        {
            var dest = Game.Map.Stockpiles.BestCellFor(guy.Carrying.Def);
            // stockpile full, drop the remainder below
            if (dest == null) break;
            var target = dest.Value;

            // walk to this stockpile cell
            yield return new Task
            {
                OnStart = () =>
                {
                    if (guy.Cell == target) return;
                    var path = Game.Pathing.GetPath(guy.Cell, target);
                    if (path == null || path.Length < 2) { _pathFailed = true; return; }
                    guy.StartPath(path);
                },
                OnTick = () => guy.MoveAlongPath(),
                IsComplete = () => guy.AtPathEnd,
                FailOn = () => _pathFailed || !Game.Map.Stockpiles.IsStockpileCell(target),
            };

            // drop as much as fits here, keep the rest
            yield return new Task
            {
                OnStart = () =>
                {
                    var existing = Game.Map.ItemAt(target, guy.Carrying.Def);
                    int room = existing == null
                        ? guy.Carrying.Def.MaxStackSize
                        : guy.Carrying.Def.MaxStackSize - existing.Count;
                    int amount = Mathf.Min(guy.Carrying.Count, room);

                    var (pile, isNew, _) = Game.Map.SpawnItem(guy.Carrying.Def, target, amount);
                    if (isNew) Game.Main.SpawnItemView(pile);

                    guy.Carrying.Count -= amount;
                    if (guy.Carrying.Count <= 0) guy.Carrying = null;
                },
                IsComplete = () => true,
                FailOn = () => !Game.Map.Stockpiles.IsStockpileCell(target),
            };
        }

        // stockpile filled up mid-haul, drop whatever's left on the floor
        if (guy.Carrying != null)
        {
            yield return new Task
            {
                OnStart = () =>
                {
                    Game.Main.DropItems(guy.Carrying.Def, guy.Cell, guy.Carrying.Count);
                    guy.Carrying = null;
                },
                IsComplete = () => true,
            };
        }
    }
}