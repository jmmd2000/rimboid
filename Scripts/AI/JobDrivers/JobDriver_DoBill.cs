using System.Collections.Generic;
using Godot;

/// <summary>
/// Works a bill at a bench: for each ingredient, gather its reserved piles in one trip and consume
/// them, then do the recipe's work and produce the output on the floor. Fails if the bench is removed.
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

        bool BenchGone() => Game.Map.BuildingAt(benchCell)?.WorkBench == null;

        // gather and consume each ingredient line (one trip per ingredient def)
        foreach (var ing in recipe.Ingredients)
        {
            var piles = job.ReservedItems.FindAll(i => i.Def == ing.Item);

            foreach (var t in GatherIntoCarrying(piles, ing.Count, failIf: BenchGone))
                yield return t;

            // carry it to a cell beside the bench
            yield return WalkTo(
                () => Game.Pathing.NearestReachableWorkCell(benchCell, guy.Cell),
                failIf: BenchGone
            );

            // consume it into the bill
            yield return new Task
            {
                OnStart = () => guy.Carrying = null,
                IsComplete = () => true,
            };
        }

        // do the cooking work, once, after all ingredients are in
        yield return new Task
        {
            OnStart = () => _work = 0,
            OnTick = () => _work += SkilledWork(recipe.Skill),
            IsComplete = () => _work >= recipe.WorkAmount,
            FailOn = () => BenchGone(),
        };

        // produce the output on the floor, the haul work-giver moves it to a stockpile
        yield return new Task
        {
            OnStart = () => Game.Map.DropItems(recipe.Output, guy.Cell, recipe.OutputCount),
            IsComplete = () => true,
        };
    }
}