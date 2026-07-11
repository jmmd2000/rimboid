public static class ToolDefOf
{
    public static ToolDef Mine;
    public static ToolDef Stockpile;
    public static ToolDef GrowZone;
    public static ToolDef Harvest;
    public static ToolDef Chop;
    public static ToolDef Deconstruct;

    /// <summary>Binds the named tool defs from the database.</summary>
    public static void Resolve()
    {
        Mine = DefDatabase<ToolDef>.Get("Mine");
        Stockpile = DefDatabase<ToolDef>.Get("Stockpile");
        GrowZone = DefDatabase<ToolDef>.Get("GrowZone");
        Harvest = DefDatabase<ToolDef>.Get("Harvest");
        Chop = DefDatabase<ToolDef>.Get("Chop");
        Deconstruct = DefDatabase<ToolDef>.Get("Deconstruct");
    }
}