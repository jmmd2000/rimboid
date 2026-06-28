using System.Collections.Generic;
using Godot;

/// <summary>
/// Works a bill at a bench: fetch each ingredient and consume it, do the recipe's work,
/// then produce the output on the floor for hauling. Fails if the bench is removed mid-job.
/// </summary>
public class JobDriver_DoBill : JobDriver
{
    float _work;

    protected override IEnumerable<Task> MakeTasks()
    {
        var bill = job.TargetBill;
        if (bill?.Recipe == null) yield break;
        var recipe = bill.Recipe;
        var benchCell = job.TargetCell;

        bool BenchGone() => Game.Map.BuildingAt(benchCell)?.Def.WorkBench == null;

        // gather and consume each ingredient line
        foreach (var ing in recipe.Ingredients)
        {
            Item source = null;

            // walk to a pile
            yield return WalkTo(
                () => { source = FindPile(ing); return source?.Cell; },
                failIf: () => BenchGone() || source == null
            );

            // pick up the needed amount
            yield return new Task
            {
                OnStart = () =>
                {
                    if (source == null) return;
                    if (ing.Count >= source.Count)
                    {
                        guy.Carrying = source;
                        Game.Map.RemoveItem(source);
                        Game.Views.RemoveItemView(source);
                    }
                    else
                    {
                        source.Count -= ing.Count;
                        guy.Carrying = new Item { Def = source.Def, Count = ing.Count };
                    }
                },
                IsComplete = () => true,
            };

            // carry it to a cell beside the bench
            yield return WalkTo(
                () => Game.Pathing.NearestReachableWorkCell(benchCell, guy.Cell),
                failIf: () => BenchGone()
            );

            // consume it into the bill
            yield return new Task
            {
                OnStart = () => guy.Carrying = null,
                IsComplete = () => true,
            };
        }

        // do the cooking work over time
        yield return new Task
        {
            OnStart = () => _work = 0,
            OnTick = () => _work += 1f,
            IsComplete = () => _work >= recipe.WorkAmount,
            FailOn = () => BenchGone(),
        };

        // produce the output on the floor
        yield return new Task
        {
            OnStart = () =>
            {
                var (pile, isNew, _) = Game.Map.SpawnItem(recipe.Output, guy.Cell, recipe.OutputCount);
                if (isNew) Game.Views.SpawnItemView(pile);
            },
            IsComplete = () => true,
        };

        /// <summary>Nearest reachable pile holding enough of an ingredient.</summary>
        Item FindPile(IngredientCount ing)
        {
            var reachable = Game.Pathing.ReachableCells(guy.Cell);
            Item best = null;
            int bestDist = int.MaxValue;
            foreach (var item in Game.Map.LooseItems)
            {
                if (item.Def != ing.Item || item.Count < ing.Count) continue;
                if (!reachable.Contains(item.Cell)) continue;
                int dist = Grid.DistanceSquared(guy.Cell, item.Cell);
                if (dist < bestDist) { bestDist = dist; best = item; }
            }
            return best;
        }
    }
}