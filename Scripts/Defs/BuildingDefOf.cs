/// <summary>Code-referenced building defs, bound from the database. Call Resolve() after DefLoader.LoadAll().</summary>
public static class BuildingDefOf
{
    public static BuildingDef WallStone;
    public static BuildingDef Stove;

    /// <summary>Binds the named building defs from the database.</summary>
    public static void Resolve()
    {
        WallStone = DefDatabase<BuildingDef>.Get("WallStone");
        Stove = DefDatabase<BuildingDef>.Get("Stove");
    }
}
