/// <summary>Scans the world for one category of work and returns a job if available.</summary>
public abstract class WorkGiver
{
    public abstract Job TryGiveJob(Guy guy);
}