/// <summary>Sends a tired colonist to sleep when rest drops below the threshold.</summary>
public class ThinkNode_Sleep : IThinkNode
{
    readonly bool _urgent;

    /// <summary>Creates a sleep node.</summary>
    /// <param name="urgent">True for a critical tier.</param>
    public ThinkNode_Sleep(bool urgent) => _urgent = urgent;

    public bool Interrupts => _urgent;

    public Job TryGiveJob(Guy guy)
    {
        float threshold = _urgent ? guy.Needs.Rest.CollapseThreshold : guy.Needs.Rest.TiredThreshold;
        return guy.Needs.Rest.Level < threshold ? new Job { Type = JobType.Sleep } : null;
    }
}