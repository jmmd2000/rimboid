using System;
using System.Collections.Generic;

/// <summary>Runtime registry of all defs of one type, keyed by DefName. Populated at startup by DefLoader.</summary>
public static class DefDatabase<T> where T : Def
{
    static readonly Dictionary<string, T> _byName = new();

    /// <summary>Every loaded def of this type.</summary>
    public static IReadOnlyCollection<T> All => _byName.Values;

    /// <summary>Looks up a def by name, or null if none registered.</summary>
    public static T Get(string defName) => _byName.TryGetValue(defName, out var def) ? def : null;

    /// <summary>Registers a def (overwrites any with the same name).</summary>
    public static void Add(T def)
    {
        if (def != null && !string.IsNullOrEmpty(def.DefName)) _byName[def.DefName] = def;
    }

    /// <summary>Clears the registry.</summary>
    public static void Clear() => _byName.Clear();
}