/// <summary>A single branch in the think tree. Returns a job or null to pass.</summary>
public interface IThinkNode
{
    Job TryGiveJob(Guy guy);

    /// <summary>True if this node may pre-empt an active job (a critical need). Default, only runs when idle.</summary>
    bool Interrupts => false;
}