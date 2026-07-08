using System;
using System.Collections.Generic;

/// <summary>One attribute's progress, an integer level and the XP banked toward the next.</summary>
public class Attribute
{
    public readonly AttributeDef Def;
    public int Level = Attributes.Baseline;
    public float XP;

    public Attribute(AttributeDef def) => Def = def;
}

/// <summary>Holds a colonist's attributes, the broad innate limits that feed the stat chain.
/// Levels sit around a baseline: the factor is 1.0 at baseline, below penalises, above boosts.</summary>
public class Attributes
{
    /// <summary>The neutral level, where an attribute's factor is exactly 1.</summary>
    public const int Baseline = 5;

    /// <summary>Highest reachable level.</summary>
    public const int MaxLevel = 20;

    /// <summary>Multiplier gained (or lost) per level away from the baseline.</summary>
    public const float FactorPerLevel = 0.05f;

    /// <summary>XP needed to climb from the given level to the next. Much steeper than skills, attributes creep.</summary>
    public static float XPToLevel(int level) => 400f * (level + 1);

    readonly Dictionary<AttributeDef, Attribute> _attributes = new();

    /// <summary>Seeds a record for every loaded attribute def so the full set is always available.</summary>
    public Attributes()
    {
        foreach (var def in DefDatabase<AttributeDef>.All)
            _attributes[def] = new Attribute(def);
    }

    /// <summary>Every attribute record, for the inspector.</summary>
    public IEnumerable<Attribute> All => _attributes.Values;

    /// <summary>Returns the record for a def, creating one if it wasn't seeded.</summary>
    public Attribute Get(AttributeDef def) => _attributes.TryGetValue(def, out var attribute) ? attribute : _attributes[def] = new Attribute(def);

    /// <summary>The stat-chain multiplier for an attribute: 1.0 at the baseline, ±FactorPerLevel per level away.
    /// A null def means no attribute term, factor 1, keeping unlinked skills and def-less tests neutral.</summary>
    public float Factor(AttributeDef def) => def == null ? 1f : 1f + (Get(def).Level - Baseline) * FactorPerLevel;

    /// <summary>Stores XP into an attribute, rolling over into levels while there's enough.</summary>
    /// <param name="def">Which attribute to train, or null for none.</param>
    /// <param name="xp">How much XP to add.</param>
    public void Gain(AttributeDef def, float xp)
    {
        if (def == null) return;
        var attribute = Get(def);
        attribute.XP += xp;
        while (attribute.Level < MaxLevel && attribute.XP >= XPToLevel(attribute.Level))
        {
            attribute.XP -= XPToLevel(attribute.Level);
            attribute.Level++;
        }
        if (attribute.Level >= MaxLevel) attribute.XP = XPToLevel(attribute.Level);
    }

    /// <summary>Rolls each attribute's starting level around the baseline (+/-2)</summary>
    /// <param name="rng">The random source, seed-derived so spawns are reproducible.</param>
    public void Roll(Random rng)
    {
        foreach (var attribute in _attributes.Values)
            attribute.Level = Baseline + rng.Next(-2, 3); // Next's max is exclusive, so -2...+2
    }
}