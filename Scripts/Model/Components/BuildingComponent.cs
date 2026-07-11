/// <summary>Runtime state and behaviour for one building capability, created from its ComponentProperties.</summary>
public abstract class BuildingComponent
{
    public Building Building;
    public ComponentProperties Props;

    /// <summary>True if this component needs a per-tick update (it gets registered into the map's tick list).</summary>
    public virtual bool Ticks => false;

    /// <summary>Per sim-tick update. Only called when Ticks is true.</summary>
    public virtual void Tick() { }
}
