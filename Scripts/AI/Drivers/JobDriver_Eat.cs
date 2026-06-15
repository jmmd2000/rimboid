using System.Collections.Generic;

/// <summary>
/// Job driver for eating. Walks to a food item and eats it a unit at a time,
/// raising the food need per unit, until full or the food runs out.
/// </summary>
public class JobDriver_Eat : JobDriver
{
    // how long one unit takes to eat
    const float TicksPerBite = 20f;

    float _biteProgress;

    protected override IEnumerable<Task> MakeTasks()
    {
        // walk to the food
        yield return WalkTo(job.TargetCell, failIf: () => !Game.Map.HasItem(job.TargetItem));

        // eat one unit at a time until full or the food is gone
        yield return new Task
        {
            OnTick = () =>
            {
                _biteProgress += 1f;
                if (_biteProgress < TicksPerBite) return;
                _biteProgress = 0f;

                guy.Needs.Food.Add(job.TargetItem.Def.Nutrition);
                job.TargetItem.Count--;
                if (job.TargetItem.Count <= 0)
                {
                    Game.Map.RemoveItem(job.TargetItem);
                    Game.Views.RemoveItemView(job.TargetItem);
                }
            },
            IsComplete = () => guy.Needs.Food.Level >= 0.99f || !Game.Map.HasItem(job.TargetItem),
        };
    }
}