using Godot;

/// <summary>A player tool applied by dragging a rectangle. Each cell gets Apply (left-drag)
/// or Cancel (right-drag); OnDragFinished runs once when the drag completes.</summary>
public abstract class Designator
{
    /// <summary>Applies the tool to one cell.</summary>
    public abstract void Apply(Vector2I cell);

    /// <summary>Clears the tool's mark from one cell.</summary>
    public abstract void Cancel(Vector2I cell);

    /// <summary>Runs once after the drag finishes, for any whole-rectangle cleanup.</summary>
    public virtual void OnDragFinished() { }
}