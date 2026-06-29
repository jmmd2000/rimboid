/// <summary>Code-referenced recipe defs, bound from the database. Call Resolve() after DefLoader.LoadAll().</summary>
public static class RecipeDefOf
{
    public static RecipeDef CookSimpleMeal;

    /// <summary>Binds the named recipe defs from the database.</summary>
    public static void Resolve()
    {
        CookSimpleMeal = DefDatabase<RecipeDef>.Get("CookSimpleMeal");
    }
}
