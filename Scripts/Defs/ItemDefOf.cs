/// <summary>Code-referenced item defs, bound from the database. Call Resolve() after DefLoader.LoadAll().</summary>
public static class ItemDefOf
{
    public static ItemDef Stone;
    public static ItemDef Berries;
    public static ItemDef Wheat;
    public static ItemDef SimpleMeal;

    /// <summary>Binds the named item defs from the database.</summary>
    public static void Resolve()
    {
        Stone = DefDatabase<ItemDef>.Get("Stone");
        Berries = DefDatabase<ItemDef>.Get("Berries");
        Wheat = DefDatabase<ItemDef>.Get("Wheat");
        SimpleMeal = DefDatabase<ItemDef>.Get("SimpleMeal");
    }
}
