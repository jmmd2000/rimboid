/// <summary>Code-referenced attribute defs, bound from the database. Call Resolve() after DefLoader.LoadAll().</summary>
public static class AttributeDefOf
{
    public static AttributeDef Strength;
    public static AttributeDef Agility;
    public static AttributeDef Endurance;
    public static AttributeDef Perception;
    public static AttributeDef Intelligence;
    public static AttributeDef Charisma;

    /// <summary>Binds the named attribute defs from the database.</summary>
    public static void Resolve()
    {
        Strength = DefDatabase<AttributeDef>.Get("Strength");
        Agility = DefDatabase<AttributeDef>.Get("Agility");
        Endurance = DefDatabase<AttributeDef>.Get("Endurance");
        Perception = DefDatabase<AttributeDef>.Get("Perception");
        Intelligence = DefDatabase<AttributeDef>.Get("Intelligence");
        Charisma = DefDatabase<AttributeDef>.Get("Charisma");
    }
}