using System.Collections.Generic;
using System.Linq;

/// <summary> Tracks which Guy has claimed which work target, so two Guys don't grab the same job.</summary>
public class ReservationManager
{
    readonly Dictionary<object, Guy> _claims = new();

    public bool Available(object target, Guy guy) => target == null || !_claims.TryGetValue(target, out var owner) || owner == guy;

    public void Reserve(object target, Guy guy)
    {
        if (target != null) _claims[target] = guy;
    }

    public void ReleaseAll(Guy guy)
    {
        foreach (var target in _claims.Where(c => c.Value == guy).Select(c => c.Key).ToList())
        {
            _claims.Remove(target);
        }
    }
}