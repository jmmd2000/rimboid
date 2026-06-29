/// <summary>Code-referenced terrain defs, bound from the database. Call Resolve() after DefLoader.LoadAll().</summary>
public static class TerrainDefOf
{
    public static TerrainDef Water;
    public static TerrainDef Stone;
    public static TerrainDef Grass;
    public static TerrainDef Dirt;

    /// <summary>Binds the named terrain defs from the database.</summary>
    public static void Resolve()
    {
        Water = DefDatabase<TerrainDef>.Get("Water");
        Stone = DefDatabase<TerrainDef>.Get("Stone");
        Grass = DefDatabase<TerrainDef>.Get("Grass");
        Dirt = DefDatabase<TerrainDef>.Get("Dirt");
    }
}
