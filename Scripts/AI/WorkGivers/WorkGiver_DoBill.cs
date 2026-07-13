using System.Collections.Generic;
using Godot;

/// <summary>Finds a workbench with a bill that should run and whose ingredients can be gathered,
/// reserves those piles, and returns a job to work that bill.</summary>
public class WorkGiver_DoBill : WorkGiver
{
    public override Job TryGiveJob(Guy guy)
    {
        var reachable = Game.Pathing.ReachableCells(guy.Cell);

        // a guy works the bench from a cell beside its footprint
        bool CanReach(Building b)
        {
            foreach (var cell in b.OccupiedCells)
                foreach (var d in Grid.Adjacent8)
                    if (reachable.Contains(cell + d)) return true;
            return false;
        }

        // a building registers under every footprint cell, so dedupe to distinct benches
        var benches = new HashSet<Building>();
        foreach (var b in Game.Map.Buildings.Values)
            if (b.WorkBench != null) benches.Add(b);

        Building bestBench = null;
        Bill bestBill = null;
        List<Item> bestPicks = null;
        int bestDist = int.MaxValue;

        foreach (var bench in benches)
        {
            if (!Game.Map.Reservations.AvailableCell(bench.Cell, guy)) continue;
            if (!CanReach(bench)) continue;

            foreach (var bill in bench.WorkBench.Bills)
            {
                if (!bill.ShouldDo) continue;
                var picks = SelectIngredients(bill.Recipe, reachable, guy);
                if (picks == null) continue;

                int dist = Grid.DistanceSquared(guy.Cell, bench.Cell);
                if (dist < bestDist) { bestDist = dist; bestBench = bench; bestBill = bill; bestPicks = picks; }
                break;
            }
        }
        if (bestBench == null) return null;

        return new Job { Type = JobType.DoBill, TargetCell = bestBench.Cell, TargetBill = bestBill, ReservedItems = bestPicks, ClaimsCell = true };
    }

    /// <summary>Picks reachable, unreserved piles covering every ingredient line, or null if any line can't be met.</summary>
    static List<Item> SelectIngredients(RecipeDef recipe, HashSet<Vector2I> reachable, Guy guy)
    {
        var picks = new List<Item>();
        foreach (var ing in recipe.Ingredients)
        {
            int total = 0;
            foreach (var item in Game.Map.LooseItemsOfDef(ing.Item))
            {
                if (total >= ing.Count) break;
                if (!reachable.Contains(item.Cell)) continue;
                if (!Game.Map.Reservations.AvailableItem(item, guy)) continue;
                picks.Add(item);
                total += item.Count;
            }
            if (total < ing.Count) return null;
        }
        return picks;
    }
}