using System.Collections.Generic;

/// <summary>Prioritised list of think nodes. Returns the first job any node offers.</summary>
public class ThinkTree
{
    readonly List<IThinkNode> _nodes = new()
    {
        new ThinkNode_Sleep(),
        new ThinkNode_Work(),
        new ThinkNode_Wander(),
    };

    /// <summary>Loops the nodes top to bottom and returns the first non-null job.</summary>
    /// <param name="guy">The colonist needing a job.</param>
    /// <returns>A job to run, or null if no node offered one.</returns>
    public Job FindJob(Guy guy)
    {
        foreach (var node in _nodes)
        {
            var job = node.TryGiveJob(guy);
            if (job != null) return job;
        }
        return null;
    }
}