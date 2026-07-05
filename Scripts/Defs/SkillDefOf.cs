public static class SkillDefOf
{
    public static SkillDef Mining;
    public static SkillDef Construction;
    public static SkillDef Farming;
    public static SkillDef Cooking;
    public static SkillDef Scavenging;

    /// <summary>Binds the named skill defs from the database.</summary>
    public static void Resolve()
    {
        Mining = DefDatabase<SkillDef>.Get("Mining");
        Construction = DefDatabase<SkillDef>.Get("Construction");
        Farming = DefDatabase<SkillDef>.Get("Farming");
        Cooking = DefDatabase<SkillDef>.Get("Cooking");
        Scavenging = DefDatabase<SkillDef>.Get("Scavenging");
    }
}