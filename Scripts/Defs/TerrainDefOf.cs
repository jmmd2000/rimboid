using Godot;

public static class TerrainDefOf
{
    public static TerrainDef Water;
    public static TerrainDef Stone;
    public static TerrainDef Grass;
    public static TerrainDef Dirt;

    public static void Load()
    {
        Water = GD.Load<TerrainDef>("res://Defs/Terrain/Water.tres");
        Stone = GD.Load<TerrainDef>("res://Defs/Terrain/Stone.tres");
        Grass = GD.Load<TerrainDef>("res://Defs/Terrain/Grass.tres");
        Dirt = GD.Load<TerrainDef>("res://Defs/Terrain/Dirt.tres");
    }
}