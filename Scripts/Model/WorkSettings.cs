using System.Collections.Generic;

/// <summary>Which work-givers a colonist will take jobs from. Default: all on. Stores only the
/// disabled ones, so a fresh colonist works everything.</summary>
public class WorkSettings
{
    readonly HashSet<WorkType> _disabled = new();

    /// <summary>True if the colonist will take work of this type.</summary>
    public bool Allows(WorkType type) => !_disabled.Contains(type);

    /// <summary>Turns a work type on or off for this colonist.</summary>
    public void Set(WorkType type, bool enabled)
    {
        if (enabled) _disabled.Remove(type);
        else _disabled.Add(type);
    }
}