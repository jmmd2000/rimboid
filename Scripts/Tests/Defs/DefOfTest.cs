using System.Collections.Generic;
using System.Reflection;
using Godot;
using GdUnit4;
using static GdUnit4.Assertions;

/// <summary>Guards that every code-referenced DefOf field resolves after <see cref="DefLoader.LoadAll"/>.
/// A missing or misnamed .tres (or a field the Resolve method forgot to bind) fails here, by name, instead
/// of surfacing as a null-reference crash deep in the game. Reflection-driven, so new DefOf fields are
/// covered automatically.</summary>
[TestSuite]
[RequireGodotRuntime]
public class DefOfTest
{
    static readonly System.Type[] DefOfTypes =
    {
        typeof(TerrainDefOf), typeof(ItemDefOf), typeof(BuildingDefOf), typeof(PlantDefOf),
        typeof(RecipeDefOf), typeof(AttributeDefOf), typeof(SkillDefOf), typeof(ToolDefOf),
    };

    [TestCase]
    public void EveryDefOfFieldResolves()
    {
        DefLoader.LoadAll();

        var missing = new List<string>();
        foreach (var type in DefOfTypes)
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (!typeof(Def).IsAssignableFrom(field.FieldType)) continue; // only the def references
                if (field.GetValue(null) == null) missing.Add($"{type.Name}.{field.Name}");
            }

        if (missing.Count > 0) GD.PushError("Unresolved DefOf fields: " + string.Join(", ", missing));
        AssertInt(missing.Count).IsEqual(0);
    }
}
