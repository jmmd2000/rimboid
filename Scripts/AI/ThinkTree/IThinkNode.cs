/// <summary>A single branch in the think tree. Returns a job or null to pass.</summary>
public interface IThinkNode
{
    Job TryGiveJob(Guy guy);
}