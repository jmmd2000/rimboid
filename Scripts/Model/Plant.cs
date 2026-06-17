using Godot;

/// <summary>A plant on the map.</summary>
public class Plant
{
    public PlantDef Def { get; init; }
    public Vector2I Cell { get; init; }

    public float DrawWidth { get; init; }
    public float DrawHeight { get; init; }

    static readonly System.Random _rng = new();

    /// <summary>Creates a plant at a cell, randomising its draw size from the def's ranges.
    /// A range whose max is 0 falls back to the footprint size.</summary>
    public static Plant Spawn(PlantDef def, Vector2I cell) => new()
    {
        Def = def,
        Cell = cell,
        DrawWidth = Randomise(def.MinDrawWidth, def.MaxDrawWidth, def.FootprintWidth),
        DrawHeight = Randomise(def.MinDrawHeight, def.MaxDrawHeight, def.FootprintHeight),
    };

    static float Randomise(float min, float max, int fallback)
    {
        if (max <= 0f) return fallback;
        return min + (float)_rng.NextDouble() * (max - min);
    }
}