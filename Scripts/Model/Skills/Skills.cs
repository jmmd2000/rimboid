using System.Collections.Generic;
using Godot;

/// <summary>One skill's progress, an integer level and the XP banked toward the next.</summary>
public class Skill
{
    public readonly SkillDef Def;
    public int Level;
    public float XP;

    public Skill(SkillDef def) => Def = def;
}

/// <summary>Holds a colonist's skills, banks XP earned by doing work, and levels them up.</summary>
public class Skills
{
    /// <summary>XP awarded each time a skilled job completes. Tuning knob.</summary>
    public const float XPPerJob = 35f;

    /// <summary>Highest reachable level.</summary>
    public const int MaxLevel = 20;

    /// <summary>XP needed to climb from the given level to the next.</summary>
    public static float XPToLevel(int level) => 100f * (level + 1);

    readonly Dictionary<SkillDef, Skill> _skills = new();

    /// <summary>Seeds a record for every loaded skill def so the full set is always available.</summary>
    public Skills()
    {
        foreach (var def in DefDatabase<SkillDef>.All)
            _skills[def] = new Skill(def);
    }

    /// <summary>Every skill record, for the inspector.</summary>
    public IEnumerable<Skill> All => _skills.Values;

    /// <summary>Returns the record for a def, creating one if it wasn't seeded.</summary>
    public Skill Get(SkillDef def) => _skills.TryGetValue(def, out var skill) ? skill : _skills[def] = new Skill(def);

    /// <summary>Banks XP into a skill, rolling over into levels while there's enough.</summary>
    /// <param name="def">Which skill to train, or null for an unskilled job.</param>
    /// <param name="xp">How much XP to add.</param>
    public void Gain(SkillDef def, float xp)
    {
        if (def == null) return;
        var skill = Get(def);
        skill.XP += xp;
        while (skill.Level < MaxLevel && skill.XP >= XPToLevel(skill.Level))
        {
            skill.XP -= XPToLevel(skill.Level);
            skill.Level++;
        }
    }
}