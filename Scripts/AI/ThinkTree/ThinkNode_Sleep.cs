/// <summary>Sends a tired colonist to sleep when rest drops below the threshold.</summary>
public class ThinkNode_Sleep : IThinkNode
{
    const float TiredThreshold = 0.3f;

    public Job TryGiveJob(Guy guy) => guy.Needs.Rest.Level < TiredThreshold ? new Job { Type = JobType.Sleep } : null;
}