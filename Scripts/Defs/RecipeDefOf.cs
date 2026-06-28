using Godot;

/// <summary>Static references to recipe definitions. Call Load() before use.</summary>
public static class RecipeDefOf
{
    public static RecipeDef CookSimpleMeal;

    /// <summary>Loads all recipe definitions from .tres resource files.</summary>
    public static void Load()
    {
        CookSimpleMeal = GD.Load<RecipeDef>("res://Defs/Recipes/CookSimpleMeal.tres");
    }
}