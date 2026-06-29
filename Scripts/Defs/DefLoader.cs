using Godot;

/// <summary>Loads def resources from folders into their DefDatabase at startup.</summary>
public static class DefLoader
{
    /// <summary>Loads every .tres in a res:// folder into DefDatabase&lt;T&gt;.</summary>
    public static void LoadFolder<T>(string path) where T : Def
    {
        using var dir = DirAccess.Open(path);
        if (dir == null) { GD.PushError($"Def folder not found: {path}"); return; }

        foreach (var file in dir.GetFiles())
        {
            // exported builds rename .tres to .tres.remap so need to strip it back
            var name = file.EndsWith(".remap") ? file.GetBaseName() : file;
            if (!name.EndsWith(".tres")) continue;

            var def = GD.Load<T>($"{path}/{name}");
            if (def != null) DefDatabase<T>.Add(def);
        }
    }

    /// <summary>Loads every def folder.</summary>
    public static void LoadAll()
    {
        LoadFolder<TerrainDef>("res://Defs/Terrain");
        LoadFolder<ItemDef>("res://Defs/Items");
        LoadFolder<BuildingDef>("res://Defs/Buildings");
        LoadFolder<PlantDef>("res://Defs/Plants");
        LoadFolder<RecipeDef>("res://Defs/Recipes");

        // these can be removed as the need for the DefOf files is dropped
        TerrainDefOf.Resolve();
        ItemDefOf.Resolve();
        BuildingDefOf.Resolve();
        PlantDefOf.Resolve();
        RecipeDefOf.Resolve();
    }
}