/// <summary>Code-referenced plant defs, bound from the database. Call Resolve() after DefLoader.LoadAll().</summary>
public static class PlantDefOf
{
    public static PlantDef Pine;
    public static PlantDef Oak;
    public static PlantDef BerryBush;
    public static PlantDef Wheat;

    /// <summary>Binds the named plant defs from the database.</summary>
    public static void Resolve()
    {
        Pine = DefDatabase<PlantDef>.Get("Pine");
        Oak = DefDatabase<PlantDef>.Get("Oak");
        BerryBush = DefDatabase<PlantDef>.Get("BerryBush");
        Wheat = DefDatabase<PlantDef>.Get("Wheat");
    }
}
