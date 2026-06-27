using Godot;

/// <summary>A plant on the map.</summary>
public class Plant
{
    public PlantDef Def { get; init; }
    public Vector2I Cell { get; init; }

    public float DrawWidth { get; set; }
    public float DrawHeight { get; set; }

    public long GrowthStartTick { get; set; }
    public long MatureAtTick { get; set; }

    public bool IsHarvestable => Def.GrowthStages.Length > 0 && GameTime.Ticks >= MatureAtTick;

    public Texture2D CurrentTexture => Def.GrowthStages.Length > 0 ? Def.GrowthStages[StageIndex] : null;

    /// <summary>Begins growth from the current tick, maturing after the given number of days.
    /// Used when a crop is sown at stage 0, or when a harvested plant regrows.</summary>
    /// <param name="days">Days from now until the plant is mature.</param>
    public void StartGrowing(float days)
    {
        GrowthStartTick = GameTime.Ticks;
        MatureAtTick = GameTime.Ticks + (long)(days * GameTime.TicksPerDay);
    }

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

    /// <summary>Current growth-stage index, from how far between start and maturity the plant is.</summary>
    public int StageIndex
    {
        get
        {
            int stages = Def.GrowthStages.Length;
            if (stages <= 1) return 0;
            long span = MatureAtTick - GrowthStartTick;
            if (span <= 0) return stages - 1; // spawned mature
            float frac = Mathf.Clamp((float)(GameTime.Ticks - GrowthStartTick) / span, 0f, 1f);
            return Mathf.Min((int)(frac * (stages - 1)), stages - 1);
        }
    }

    /// <summary>Draw width for the current stage. When the def opts in, scales from the
    /// footprint at stage 0 up to the random size at the final stage, otherwise the random size.</summary>
    public float StageDrawWidth => StageScaledSize(DrawWidth, Def.FootprintWidth);

    /// <summary>Draw height for the current stage</summary>
    public float StageDrawHeight => StageScaledSize(DrawHeight, Def.FootprintHeight);

    float StageScaledSize(float matureSize, int footprint)
    {
        if (!Def.DrawSizeGrowsWithStage) return matureSize;
        int stages = Def.GrowthStages.Length;
        if (stages <= 1) return matureSize;
        float t = (float)StageIndex / (stages - 1);
        return Mathf.Lerp(footprint, matureSize, t);
    }
}