using System.Collections.Generic;
using Godot;

/// <summary>Runtime registry of all defs of one type, keyed by DefName. Populated at startup by DefLoader.</summary>
public static class DefDatabase<T> where T : Def
{
    static readonly Dictionary<string, T> _byName = new();

    /// <summary>Every loaded def of this type.</summary>
    public static IReadOnlyCollection<T> All => _byName.Values;

    /// <summary>Looks up a def by name. Throws if none is registered. 
    /// Use <see cref="TryGet"/> for optional lookups.</summary>
    public static T Get(string defName)
    {
        if (_byName.TryGetValue(defName, out var def)) return def;
        throw new KeyNotFoundException($"No {typeof(T).Name} named '{defName}'. Check the .tres exists in its Defs folder and the name matches.");
    }

    /// <summary>Looks up a def by name without throwing. Returns true and sets <paramref name="def"/> when found.</summary>
    public static bool TryGet(string defName, out T def) => _byName.TryGetValue(defName, out def);

    /// <summary>Registers a def (overwrites any with the same name).</summary>
    public static void Add(T def)
    {
        if (def == null) { GD.PushWarning($"DefDatabase<{typeof(T).Name}>.Add ignored a null def."); return; }
        if (string.IsNullOrEmpty(def.DefName)) { GD.PushWarning($"DefDatabase<{typeof(T).Name}>.Add ignored a def with an empty DefName."); return; }
        _byName[def.DefName] = def;
    }

    /// <summary>Clears the registry.</summary>
    public static void Clear() => _byName.Clear();
}