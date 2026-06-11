using Godot;

/// <summary>The kind of job; selects which driver runs it.</summary>
public enum JobType { Mine, Haul, Wander }

/// <summary>Intent data for a job. Holds targets the colonist should act on.</summary>
public class Job
{
    /// <summary>Which kind of job this is; selects the driver to run it.</summary>
    public JobType Type;
    /// <summary>Primary target cell (e.g. rock to mine, item to pick up).</summary>
    public Vector2I TargetCell;
    /// <summary>Secondary destination cell (e.g. stockpile drop-off).</summary>
    public Vector2I DestinationCell;
    /// <summary>The item this job acts on, if any.</summary>
    public Item TargetItem;
}