using System.Collections.Generic;
using Godot;

/// <summary>Finds a workbench with a bill that should run and whose ingredients are reachable,
/// and returns a job to work that bill.</summary>
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
            if (b.Def.WorkBench != null) benches.Add(b);

        Building bestBench = null;
        Bill bestBill = null;
        int bestDist = int.MaxValue;

        foreach (var bench in benches)
        {
            if (!Game.Map.Reservations.AvailableCell(bench.Cell, guy)) continue;
            if (!CanReach(bench)) continue;

            foreach (var bill in bench.WorkBench.Bills)
            {
                if (!bill.ShouldDo) continue;
                if (!IngredientsAvailable(bill.Recipe, reachable)) continue;

                int dist = Grid.DistanceSquared(guy.Cell, bench.Cell);
                if (dist < bestDist) { bestDist = dist; bestBench = bench; bestBill = bill; }
                break;
            }
        }
        if (bestBench == null) return null;

        return new Job { Type = JobType.DoBill, TargetCell = bestBench.Cell, TargetBill = bestBill, ClaimsCell = true };
    }

    /// <summary>True if every ingredient line is met by at least one reachable pile.</summary>
    static bool IngredientsAvailable(RecipeDef recipe, HashSet<Vector2I> reachable)
    {
        foreach (var ing in recipe.Ingredients)
        {
            bool found = false;
            foreach (var item in Game.Map.LooseItems)
            {
                if (item.Def == ing.Item && item.Count >= ing.Count && reachable.Contains(item.Cell)) { found = true; break; }
            }
            if (!found) return false;
        }
        return true;
    }
}