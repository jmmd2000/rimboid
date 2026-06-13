using Godot;

/// <summary>A need that decays over time and is refilled by a job. Range 0..1.</summary>
public abstract class Need
{
    /// <summary>Current satisfaction: 1 = full, 0 = empty.</summary>
    public float Level = 1f;

    /// <summary>How much the need falls each tick.</summary>
    public abstract float FallPerTick { get; }

    /// <summary>Decays the need by one tick, scaled by an exertion rate, clamped at 0.</summary>
    /// <param name="rate">Fall-rate multiplier: 1 = normal, > 1 = faster.</param>
    public virtual void Tick(float rate = 1f) => Level = Mathf.Max(0f, Level - FallPerTick * rate);

    /// <summary>Raises the need, clamped at 1.</summary>
    /// <param name="amount">How much to add.</param>
    public void Add(float amount) => Level = Mathf.Min(1f, Level + amount);
}

/// <summary>Rest need. Empties over roughly two-thirds of a day awake.</summary>
public class Need_Rest : Need
{
    public override float FallPerTick => 1f / (GameTime.TicksPerDay * 0.66f);
}

/// <summary>Food need. Empties over roughly a day before the colonist starves.</summary>
public class Need_Food : Need
{
    public override float FallPerTick => 1f / (GameTime.TicksPerDay * 0.9f);
}

/// <summary>Holds a colonist's needs and ticks them together.</summary>
public class Needs
{
    public Need_Rest Rest = new();
    public Need_Food Food = new();

    /// <summary>Decays every need by one tick. Exertion scales how fast rest drains.</summary>
    /// <param name="exertion">How hard the colonist is working: 1 = idle/resting.</param>
    public void Tick(float exertion)
    {
        Rest.Tick(exertion);
        Food.Tick();
    }
}