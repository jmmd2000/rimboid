using System.Collections.Generic;

/// <summary>Think node that delegates to work givers. Returns the first job any of them offer.</summary>
public class ThinkNode_Work : IThinkNode
{
    readonly List<WorkGiver> _workGivers = new()
    {
        new WorkGiver_Mine(),
        new WorkGiver_Chop(),
        new WorkGiver_Harvest(),
        new WorkGiver_Sow(),
        new WorkGiver_Construct(),
        new WorkGiver_Haul(),
        new WorkGiver_Consolidate(),
    };

    public Job TryGiveJob(Guy guy)
    {
        foreach (var giver in _workGivers)
        {
            var job = giver.TryGiveJob(guy);
            if (job != null) return job;
        }
        return null;
    }
}