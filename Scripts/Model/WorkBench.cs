using System.Collections.Generic;

/// <summary>Runtime state for a building that hosts bills. Created only when the def has a WorkBench capability.</summary>
public class WorkBench
{
    public List<Bill> Bills { get; } = new();
}