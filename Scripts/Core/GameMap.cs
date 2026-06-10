using System.Collections.Generic;
using System.Linq;
using Godot;

public class GameMap
{
    public int Width;
    public int Height;
    public TerrainDef[,] Terrain;
    public DesignationManager Designations = new();
    public List<Item> LooseItems = new();

    public GameMap(int width, int height)
    {
        Width = width;
        Height = height;
        Terrain = new TerrainDef[width, height];
    }

    public void SpawnItem(ItemDef def, Vector2I cell, int count)
    {
        var existing = LooseItems.FirstOrDefault(i => i.Def == def && i.Cell == cell);
        if (existing != null)
            existing.Count += count;
        else
            LooseItems.Add(new Item { Def = def, Cell = cell, Count = count });
    }
}